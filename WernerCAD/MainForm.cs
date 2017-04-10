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


namespace WernerCAD
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		List<PointF> Punkte;
		Matrix TransformationMatrix;
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void TransformPoints(PointF soll, PointF ist)
		{
			PointF[] points = Punkte.ToArray();
			TransformationMatrix.TransformPoints(points);
			TransformationMatrix.
			
		}
		void OpenFileDialog1FileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				using (StreamReader r = new StreamReader ( openFileDialog1.FileName ))
				{
					string line;
					float X, Y;
					
					while ((line = r.ReadLine()) != null)
					{
						Match matchx = Regex.Match(line, @"X([0-9\.]+)");
						Match matchy = Regex.Match(line, @"Y([0-9\.]+)");
						
						if ( matchx.Success )
						{
							String s = matchx.Groups[1].Value;
							X = Convert.ToSingle(s);
						}
						
						
						
						if ( matchy.Success )
						{
							String s = matchy.Groups[1].Value;
							Y = Convert.ToSingle(s);
						}
						
						if ( matchx.Success || matchy.Success )
							Punkte.Add ( new PointF ( X, Y ) );
					}
				}
			}
			
			catch ( Exception ex )
			{
				MessageBox.Show ( ex.ToString());
			}
		}
		
		void Button1Click(object sender, EventArgs e)
		{
			openFileDialog1.ShowDialog ();
		}
	}
}
