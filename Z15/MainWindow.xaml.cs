﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BTree_lib;

namespace Z15
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BTree_INCC<int, Student> tree;
        string selectedFac = "";
        int selectedCourse = -1;
        public MainWindow()
        {
            InitializeComponent();

            tree = new BTree_INCC<int, Student>(100);            

            InitTree(tree);

            studentsGrid.ItemsSource = tree;

            (FindName("cb") as ComboBox).SelectionChanged += ActionSelected;

            var facBox = FindName("cbFac") as ComboBox;

            InitFilterBoxes();
        }

        private void InitFilterBoxes()
        {
            var facBox = FindName("cbFac") as ComboBox;
            facBox.ItemsSource = tree.Select(x=>x.Faculty).Distinct();
            var courseBox = FindName("cbCourse") as ComboBox;
            courseBox.ItemsSource = tree.Select(x => x.CourseNumber).Distinct();
        }

        private void ActionSelected(object sender, RoutedEventArgs e)
        {
            var bt = FindName("bt") as Button;
            var dockPanels = new DockPanel[] { idPanel, firstName, secondName, lastName, faculty, courseNumber };
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedIndex == 0)
                foreach (var el in dockPanels)
                    el.Visibility = Visibility.Visible;
            else
                foreach (var el in dockPanels.Skip(1))
                    el.Visibility = Visibility.Hidden;
            DeleteTextFromFields();   
        }

        private void FacultyChanged(object sender, RoutedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb.SelectedIndex != -1)
                selectedFac = cb.SelectedItem.ToString();
            UseFilter();
        }

        private void CourseChanged(object sender, RoutedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb.SelectedIndex != -1)
                selectedCourse = cb.SelectedIndex+1;
            UseFilter();
        }

        private void UseFilter()
        {
            if (selectedCourse != -1 && selectedFac != "")
            {
                studentsGrid.ItemsSource = tree.Where(x => x.Faculty == selectedFac && x.CourseNumber == selectedCourse);
            }
            else if (selectedCourse == -1)
            {
                studentsGrid.ItemsSource = tree.Where(x => x.Faculty == (cbFac.SelectedItem.ToString()));
            }
            else if (selectedFac == "")
            {
                studentsGrid.ItemsSource = tree.Where(x => x.CourseNumber == selectedCourse);
            }

        }

        private void Text_Changed(object sender, RoutedEventArgs e)
        {
            var bt = FindName("bt") as Button;
            var textBox = sender as TextBox;
            var isCorrect = IsCorrect(textBox);
            textBox.BorderBrush = isCorrect ? Brushes.Green :Brushes.Red;
            bt.IsEnabled = (FindName("cb") as ComboBox).SelectedIndex == 0 ? IsAllCorrect() : IsCorrect(idPanelText);
        }

        private bool IsAllCorrect()
        {
            return IsCorrect(idPanelText)
                && IsCorrect(courseNumberText)
                && IsCorrect(lastNameText)
                && IsCorrect(firstNameText)
                && IsCorrect(facultyText);
        }

        private bool IsCorrect(TextBox textBox)
        {
            var cb = FindName("cb") as ComboBox;
            switch (textBox.Name)
            {
                case "idPanelText":
                case "courseNumberText":
                    return int.TryParse(textBox.Text, out int res);
                case "lastNameText":
                case "firstNameText":
                case "facultyText":
                    return !string.IsNullOrWhiteSpace(textBox.Text);
                default:
                    return false;
            }
        }

        private void OffFilter_Click(object sender, RoutedEventArgs e)
        {
            studentsGrid.ItemsSource = tree;
            cbFac.SelectedIndex = -1;
            cbCourse.SelectedIndex = -1;
            selectedCourse = -1;
            selectedFac = "";
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            var box = FindName("cb") as ComboBox;
            if (box.SelectedIndex == 0)
            {
                var st = new Student(int.Parse(idPanelText.Text),
                    lastNameText.Text,
                    firstNameText.Text,
                    secondNameText.Text,
                    facultyText.Text,
                    int.Parse(courseNumberText.Text));
                tree.Insert(st.id, st);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(idPanelText.Text))
                    MessageBox.Show("Введите корректное значение в поле.");
                else
                {
                    if (int.TryParse(idPanelText.Text, out int id))
                    {
                        if (!tree.Contains(id))
                        {
                            MessageBox.Show("Такого значения не существует.", "Ошибка");
                            return;
                        }
                        if (box.SelectedIndex == 1)
                        {
                            tree.Delete(id);
                        }
                        else
                        {
                            studentsGrid.ItemsSource = tree.Where(st => st.id == id);
                        }
                    }
                    else
                        MessageBox.Show("Введите коректное значение в поле.", "Ошибка");
                    
                }
            }
            DeleteTextFromFields();
        }

        private void DeleteTextFromFields()
        {
            var texts = new[] { idPanelText, firstNameText, lastNameText, secondNameText, facultyText, courseNumberText };
            foreach (var e in texts)
            {                
                e.Text = "";
                e.BorderBrush = Brushes.Gray;
            }
            bt.IsEnabled = (FindName("cb") as ComboBox).SelectedIndex == 0 ? IsAllCorrect() : IsCorrect(idPanelText);
        }

        void InitBD(ObservableCollection<Student> students, BTree_INCC<int, Student> tree)
        {
            var lines = File.ReadAllLines(@".\students.txt");
            var st = lines.Skip(1).Select(x => x.Split(';')).ToArray();
            for (int i = 0; i < st.Count(); i++)
                students.Add(new Student(int.Parse(st[i][0]), st[i][1], st[i][2], st[i][3], st[i][4], int.Parse(st[i][5])));
            InitTree(tree);
        }

        void InitTree(BTree_INCC<int, Student> tree)
        {
            foreach (var line in File.ReadAllLines(@".\students.txt").Skip(1).Select(x => x.Split(';')))
            {
                var st = new Student(int.Parse(line[0]), line[1], line[2], line[3], line[4], int.Parse(line[5]));
                tree.Insert(st.id, st);
            }
        }

    }
    

}
