using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace ANP_Helper
{
    class FileManager
    {


        public static string getANPFilePath()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            bool? response = fileDialog.ShowDialog();

            if (response == true)
            {
                string filepath = fileDialog.FileName;

                try
                {
                    string ext = filepath.Split(".")[1];

                    if (ext != "anp")
                    {
                        MessageBox.Show("To nie jest plik ANP!");
                        return null;
                    }

                }
                catch (IndexOutOfRangeException)
                {
                    MessageBox.Show("To nie jest plik ANP!");
                    return null;
                }

                return filepath;
            }

            return null;
        }

       /* public static Dictionary<string, Trajectory> readTrajectories(string filepath)
        {
            string[] lines = System.IO.File.ReadAllLines($"{filepath}");
            Dictionary<string, Trajectory> trajectories = new Dictionary<string, Trajectory>();

            foreach (string line in lines)
            {
                if(line.StartsWith("definiuj"))
                {
                    try
                    {
                        string[] lineSegments = line.Split(" ");

                        Trajectory t = new Trajectory()
                        {
                            name = lineSegments[1],
                            directiveString = line.Split("\"")[1],
                            trains = new Dictionary<int, Directive>()
                        };

                        trajectories.Add(lineSegments[1], t);
                    } catch(IndexOutOfRangeException)
                    {
                        MessageBox.Show("Ten plik nie jest poprawnym plikiem ANP!");
                        return null;
                    }
                }
            }

            return trajectories;
        }*/
    }
}
