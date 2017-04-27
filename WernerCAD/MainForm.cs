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

namespace WernerCAD
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
        List<Tuple<String, PointF?>> Lines = new List<Tuple<string, PointF?>> ();

		Matrix TransformationMatrix;

        OpenFileDialog OpenDialog;
        SaveFileDialog SaveDialog;


        public MainForm()
		{
			InitializeComponent();


            SaveDialog = new SaveFileDialog ();
            SaveDialog.Title = "Destination File";
            SaveDialog.Filter = "TAP-Dateien(*.tap)|*.tap";


            OpenDialog = new OpenFileDialog ();
            OpenDialog.Title = "Source File";
            OpenDialog.Filter = "TAP-Dateien(*.tap)|*.tap";
        }
        void TransformPoints()
        {
            List<PointF> list = new List<PointF> ();
            foreach (Tuple<String, PointF?> l in Lines)
            {
                if (l.Item2 != null)
                {
                    list.Add ( (PointF)l.Item2 );
                }
            }
            PointF[] pointArr = list.ToArray ();
            TransformationMatrix.TransformPoints ( pointArr );

            List<Tuple<String, PointF?>> newLines = new List<Tuple<string, PointF?>> ();

            int i = 0;
            foreach (Tuple<String, PointF?> l in Lines)
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

            Lines = newLines;
        }

		
		void CalculateMatrix(PointF soll, PointF ist)
		{
            TransformationMatrix = new Matrix ();
            double angleRad = Math.Atan2 ( soll.X * ist.Y - soll.Y * ist.X, soll.X * ist.X + soll.Y * ist.Y );
            double angleDeg = angleRad * 180 / Math.PI;
            TransformationMatrix.RotateAt ( -(float)angleDeg, PointF.Empty );
            Debug.WriteLine ( "Angle: " + angleDeg );
		}

		void ReadFile ()
		{
			try
			{
				using (StreamReader r = new StreamReader ( SourceFileLabel.Text ))
				{
					string line;
					float lastX = 0, lastY = 0;
                    
					while ((line = r.ReadLine()) != null)
					{
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

                        Lines.Add ( new Tuple <string, PointF?> ( other, point ) );
                    }
				}
			}
			
			catch ( Exception ex )
			{
				MessageBox.Show ( ex.ToString());
			}


            try
            {
                CalculateMatrix ( new PointF ( float.Parse ( textBoxSollX.Text ), float.Parse ( textBoxSollY.Text ) ), new PointF ( float.Parse ( textBoxIstX.Text ), float.Parse ( textBoxIstY.Text ) ) );
                TransformPoints ();
            }

            catch (Exception ex)
            {
                MessageBox.Show ( ex.ToString () );
            }
        }

        void Button1Click ( object sender, EventArgs e )
        {
            ReadFile ();

            try
            {
                progressBar1.Value = 50;

                string output = "";
                foreach (var l in Lines)
                {
                    String line = "";
                    line += l.Item1;
                    if (l.Item2 != null)
                    {
                        line += " X" + l.Item2.Value.X.ToString ( CultureInfo.InvariantCulture );
                        line += " Y" + l.Item2.Value.Y.ToString ( CultureInfo.InvariantCulture );
                    }

                    output += line + Environment.NewLine;
                }

                if (File.Exists ( SaveDialog.FileName ))
                    File.Delete ( SaveDialog.FileName );

                File.WriteAllText ( SaveDialog.FileName, output );

                progressBar1.Value = 100;

                MessageBox.Show ( "Successfully converted .TAP File!" );
            }

            catch (Exception ex)
            {
                MessageBox.Show ( ex.ToString () );
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
