/*
 * Created by SharpDevelop.
 * User: 12zanmat
 * Date: 06.04.2017
 * Time: 13:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace WernerCAD
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
        OpenFileDialog OpenDialog;
        SaveFileDialog SaveDialog;

        public MainForm()
		{
			InitializeComponent();

            SaveDialog = new SaveFileDialog ();
            SaveDialog.Title = "Destination File";
            SaveDialog.Filter = "TAP-Dateien(*.tap)|*.tap|GCode-Dateien(*.gcode)|*.gcode";

            OpenDialog = new OpenFileDialog ();
            OpenDialog.Title = "Source File";
            OpenDialog.Filter = "TAP-Dateien(*.tap)|*.tap|GCode-Dateien(*.gcode)|*.gcode";
        }

        List<Tuple<string, PointF?>> TransformLines(Matrix matrix, List<Tuple<string, PointF?>> lines)
        {
            List<PointF> list = new List<PointF> ();
            foreach (var l in lines)
            {
                if (l.Item2 != null)
                {
                    list.Add ( (PointF)l.Item2 );
                }
            }
            PointF[] pointArr = list.ToArray ();
            matrix.TransformPoints ( pointArr );

            List<Tuple<String, PointF?>> newLines = new List<Tuple<string, PointF?>> ();

            int i = 0;
            foreach (var l in lines)
            {
                if (l.Item2 != null)
                {
                    newLines.Add ( new Tuple<string, PointF?> ( l.Item1, pointArr[i++] ) );
                }

                else
                {
                    newLines.Add ( new Tuple<string, PointF?> ( l.Item1, null ) );
                }
            }


            return newLines;
        }

		Matrix CalculateMatrix(PointF soll, PointF ist)
		{
            Matrix transformationMatrix = new Matrix ();
            double angleRad = Math.Atan2 ( soll.X * ist.Y - soll.Y * ist.X, soll.X * ist.X + soll.Y * ist.Y );
            double angleDeg = angleRad * 180 / Math.PI;
            transformationMatrix.RotateAt ( -(float)angleDeg, PointF.Empty );
            Debug.WriteLine ( "Angle: " + angleDeg );
            return transformationMatrix;
		}

        List<Tuple<String, PointF?>> ReadFile ()
		{
            List<Tuple<String, PointF?>> lines = new List<Tuple<string, PointF?>>();
            try
			{
				using (StreamReader r = new StreamReader ( SourceFileLabel.Text ))
				{
					string line;
					float lastX = 0, lastY = 0;
                    
					while ((line = r.ReadLine()) != null)
					{
                        if (line.Contains(';'))
                            line = line.Split(';')[0];
                        if (line == "")
                            continue;

						Match otherMatch = Regex.Match ( line, @"[A-WZ][0-9\.]+" );
                        Match xMatch = Regex.Match ( line, @"X([0-9\.]+)" );
                        Match yMatch = Regex.Match ( line, @"Y([0-9\.]+)" );

                        string other = "";

                        while ( otherMatch.Success )
						{
                            other += otherMatch.Groups[0].Value + " ";
                            otherMatch = otherMatch.NextMatch ();
                        }
                        
                        if (xMatch.Success)
                        {
                            lastX = Convert.ToSingle ( xMatch.Groups[1].Value );
                        }

                        if (yMatch.Success)
                        {
                            lastY = Convert.ToSingle ( yMatch.Groups[1].Value );
                        }

                        PointF? point;
                        if (!xMatch.Success && !yMatch.Success)
                            point = null;
                        else
                            point = new PointF ( lastX, lastY );

                        lines.Add ( new Tuple <string, PointF?> ( other, point ) );
                        
                    }
				}
			}
			
			catch ( Exception ex )
			{
				MessageBox.Show ( ex.ToString());
			}


            try
            {
                PointF soll = new PointF(float.Parse(textBoxSollX.Text), float.Parse(textBoxSollY.Text));
                PointF ist  = new PointF(float.Parse(textBoxIstX.Text), float.Parse(textBoxIstY.Text));
                Matrix matrix = CalculateMatrix(soll, ist);

                return TransformLines(matrix, lines);
                
            }

            catch (Exception ex)
            {
                MessageBox.Show ( ex.ToString () );
                return null;
            }
        }

        async void ButtonStartClick ( object sender, EventArgs e )
        {
            if (textBoxIstX.Text == "" || textBoxIstY.Text == "" || textBoxSollX.Text == "" || textBoxSollY.Text == "" || SourceFileLabel.Text == "" || DestFileLabel.Text == "")
            {
                MessageBox.Show("Please fill all required fields!");
                return;
            }
            await Task.Run(() => Process());
            
        }

        void ChangeProgress(int progress)
        {
            BeginInvoke((MethodInvoker) delegate
            {
                progressBar1.Value = progress;
            });
        }

        void Process()
        {
            ChangeProgress(20);
            List<Tuple<String, PointF?>> lines = ReadFile();

            try
            {
                ChangeProgress(50);
                String formatString = "N" + numericUpDownDecCount.Value;

                StreamWriter writer = new StreamWriter(SaveDialog.FileName);
                //string output = "";
                int i = 0;
                foreach (var l in lines)
                {
                    i++;
                    String line = "";
                    line += l.Item1;
                    if (l.Item2 != null)
                    {
                        line += " X" + l.Item2.Value.X.ToString(formatString, CultureInfo.InvariantCulture);
                        line += " Y" + l.Item2.Value.Y.ToString(formatString, CultureInfo.InvariantCulture);
                    }
                    writer.WriteLine(line);
                    ChangeProgress(50 + (i * 50 / lines.Count));
                    //output += line + Environment.NewLine;
                }
                //File.WriteAllText ( SaveDialog.FileName, output );
                ChangeProgress(100);
                MessageBox.Show("Successfully converted File!");
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                progressBar1.Value = 0;
            }
        }

        private void MainForm_Load ( object sender, EventArgs e )
        {

        }

        private void buttonOpen_Click ( object sender, EventArgs e )
        {
            if (OpenDialog.ShowDialog () == DialogResult.OK)
                SourceFileLabel.Text = OpenDialog.FileName;

        }

        private void buttonSave_Click ( object sender, EventArgs e )
        {
            if (SaveDialog.ShowDialog () == DialogResult.OK)
                DestFileLabel.Text = SaveDialog.FileName;
        }
    }
}
