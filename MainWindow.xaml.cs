/*  
    НИС "Программирование на C#". Домашнее задание 1, модуль 1
    Комментарии к заданию см. в файле README.txt
    * Коростелев Александр, БПИ258
*/

using System.Data.Common;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames.Font;

namespace Minotaur
{
    public partial class MainWindow : Window
    {
        // Константные обозначения типов клеток
        const int blankN = 0; 
        const int wallN = 1;
        const int exitN = 2;
        const int heroN = 3;
        const int minoN = 4;
        const int victN = 5;
        const int defN = 6;


        // Количество клеток на стороне - любое нечетное число, >= 5.
        static int fieldN = 13;

        // Матрица игрового поля
        static int[,] fieldArr = new int[fieldN, fieldN];

        // Координаты игрока, инициализация позже в коде
        static int iHero = 0; 
        static int jHero = 0;
        static int startI = 0; 
        static int startJ = 0;

        static int exitI = 0;
        static int exitJ = 0;

        static int moveCount = 0;

        //Флаги для обработки нажатий
        static bool wReady = true;
        static bool aReady = true;
        static bool sReady = true;
        static bool dReady = true;
        static bool rReady = true;
        static bool nReady = true;
        static bool blocked = false;

        // Объявление источников изображений
        static BitmapImage blankBitmap = new BitmapImage();
        static BitmapImage wallBitmap = new BitmapImage();
        static BitmapImage exitBitmap = new BitmapImage();
        static BitmapImage heroBitmap = new BitmapImage();
        static BitmapImage bgBitmap = new BitmapImage();
        static BitmapImage victoryBitmap = new BitmapImage();
        static BitmapImage defeatBitmap = new BitmapImage();
        static BitmapImage runBitmap = new BitmapImage();
        static BitmapImage controlsBitmap = new BitmapImage();
        static BitmapImage heroDefBitmap = new BitmapImage();
        static BitmapImage heroVictBitmap = new BitmapImage();

        static Grid fieldGrid = new Grid();

        static TextBlock tbMove = new TextBlock();
        static Image victory = new Image();
        static Image defeat = new Image();
        static Image run = new Image();

        // Генерация лабиринта
        static void GenerateField()
        {
            Random rnd = new Random();

            for (int i = 0; i < fieldN; i++)
            {
                for (int j = 0; j < fieldN; j++)
                {
                    fieldArr[i, j] = wallN;
                }
            }

            int si = rnd.Next(0, fieldN / 2) * 2 + 1; 
            int sj = rnd.Next(0, fieldN / 2) * 2 + 1;

            fieldArr[si, sj] = blankN;

            int[] di = {-2, 0, 0, +2 };
            int[] dj = {0, -2, +2, 0 };

            while (true)
            {

                bool ready = true;
                for (int i = 1; i < fieldN; i += 2)
                {
                    for (int j = 1; j < fieldN; j += 2)
                    {
                        if (fieldArr[i, j] != blankN)
                        {
                            ready = false;
                        }
                    }
                }

                if (ready) break;




                while (true)
                {
                    int dir = rnd.Next(0, 4);

                    bool fDeadEnd = true;

                    for (int j = 0; j < 4; j++)
                    {
                        int ki = si + di[j];
                        int kj = sj + dj[j];

                        if (!((ki <= 0) || (kj <= 0) || (ki >= fieldN) || (kj >= fieldN) || (fieldArr[ki, kj] == blankN)))
                        {
                            fDeadEnd = false;
                        }

                    }

                    if (fDeadEnd)
                    {

                        si = rnd.Next(0, fieldN / 2) * 2 + 1;
                        sj = rnd.Next(0, fieldN / 2) * 2 + 1;

                        while (fieldArr[si, sj] != 0)
                        {
                            si = rnd.Next(0, fieldN / 2) * 2 + 1;
                            sj = rnd.Next(0, fieldN / 2) * 2 + 1;
                        }

                        fieldArr[si, sj] = blankN;
                        break;

                    }

                    int ni = si + di[dir];  
                    int nj = sj + dj[dir];

                    if ((ni <= 0) || (nj <= 0) || (ni >= fieldN) || (nj >= fieldN) || (fieldArr[ni, nj] == blankN))
                    {
                        continue;
                    }

                    fieldArr[ni, nj] = blankN;
                    fieldArr[si + di[dir] / 2, sj + dj[dir] / 2] = blankN;

                    si = ni;
                    sj = nj;

                    
                }


            }

            // Добавить выход
            si = rnd.Next(0, fieldN / 2) * 2 + 1;
            sj = rnd.Next(0, fieldN / 2) * 2 + 1;
            while (fieldArr[si, sj] != blankN)
            {
                si = rnd.Next(0, fieldN / 2) * 2 + 1;
                sj = rnd.Next(0, fieldN / 2) * 2 + 1;
            }

            fieldArr[si, sj] = exitN;
            exitI = si; exitJ = sj;

        }
        
        static void InitHero()
        {
            Random rnd = new Random();
            iHero = rnd.Next(0, fieldN / 2) * 2 + 1;
            jHero = rnd.Next(0, fieldN / 2) * 2 + 1;
            while (fieldArr[iHero, jHero] != blankN)
            {
                iHero = rnd.Next(0, fieldN / 2) * 2 + 1;
                jHero = rnd.Next(0, fieldN / 2) * 2 + 1;
            }

            fieldArr[iHero, jHero] = heroN;

            startI = iHero;
            startJ = jHero;
        }

        static void ResetLab()
        {
            GenerateField();
            InitHero();
            moveCount = 0;
        }

        //0 - OK, 1 - Victory, 2 - Loss
        static int CheckMove(int i, int j) 
        {
            switch (fieldArr[i,j])
            {
                case blankN: { return 0; }
                case wallN: { return 2; }
                case exitN: { return 1; }
                default: return -1;
            }
        }

        
        static void RedrawField()
        {

            fieldGrid.Children.Clear();

            for (int i = 0; i < fieldN; i++)
            {
                for (int j = 0; j < fieldN; j++)
                {
                    int x = fieldArr[i, j];

                    Image filler = new Image();

                    switch (x)
                    {
                        case blankN: { filler.Source = blankBitmap; break; }
                        case wallN: { filler.Source = wallBitmap; break; }
                        case exitN: { filler.Source = exitBitmap; break; }
                        case heroN: { filler.Source = heroBitmap; break; }
                    }

                    Grid.SetColumn(filler, j);
                    Grid.SetRow(filler, i);

                    fieldGrid.Children.Add(filler);

                }
            }
        }

        static void RedrawAll()
        {
            RedrawField();
            tbMove.Text = $"Moves: {moveCount}";
            victory.Visibility = Visibility.Hidden;
            defeat.Visibility = Visibility.Hidden;
            run.Visibility = Visibility.Visible;
        }

        static void SquareRedraw(int i, int j)
        {
            Image filler = new Image();

            switch (fieldArr[i,j])
            {
                case blankN: { filler.Source = blankBitmap; break; }
                case wallN: { filler.Source = wallBitmap; break; }
                case exitN: { filler.Source = exitBitmap; break; }
                case heroN: { filler.Source = heroBitmap; break; }
                case victN: { filler.Source = heroVictBitmap; break; }
                case defN: { filler.Source = heroDefBitmap; break; }
            }

            Grid.SetColumn(filler, j);
            Grid.SetRow(filler, i);

            fieldGrid.Children.Add(filler);
        }

        public MainWindow()
        {
            InitializeComponent();

            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            // Для нахождения стороны квадрата поля/клетки
            double fieldProportion = this.Height / this.Width;

            // Подгрузка изображений

            blankBitmap.BeginInit();
            blankBitmap.UriSource = new Uri(@"resources\blank.jpg", UriKind.Relative);
            blankBitmap.EndInit();


            wallBitmap.BeginInit();
            wallBitmap.UriSource = new Uri(@"resources\wall.jpg", UriKind.Relative);
            wallBitmap.EndInit();

            exitBitmap.BeginInit();
            exitBitmap.UriSource = new Uri(@"resources\exit.png", UriKind.Relative);
            exitBitmap.EndInit();

            bgBitmap.BeginInit();
            bgBitmap.UriSource = new Uri(@"resources\bg_map.jpg", UriKind.Relative);
            bgBitmap.EndInit();

            heroBitmap.BeginInit();
            heroBitmap.UriSource = new Uri(@"resources\hero.jpg", UriKind.Relative);
            heroBitmap.EndInit();

            victoryBitmap.BeginInit();
            victoryBitmap.UriSource = new Uri(@"resources\victory.png", UriKind.Relative);
            victoryBitmap.EndInit();

            defeatBitmap.BeginInit();
            defeatBitmap.UriSource = new Uri(@"resources\defeat.png", UriKind.Relative);
            defeatBitmap.EndInit();

            controlsBitmap.BeginInit();
            controlsBitmap.UriSource = new Uri(@"resources\controls.png", UriKind.Relative);
            controlsBitmap.EndInit();

            heroDefBitmap.BeginInit();
            heroDefBitmap.UriSource = new Uri(@"resources\hero_def.jpg", UriKind.Relative);
            heroDefBitmap.EndInit();

            heroVictBitmap.BeginInit();
            heroVictBitmap.UriSource = new Uri(@"resources\hero_vict.jpg", UriKind.Relative);
            heroVictBitmap.EndInit();

            runBitmap.BeginInit();
            runBitmap.UriSource = new Uri(@"resources\run.png", UriKind.Relative);
            runBitmap.EndInit();


            // Сетка нулевого уровня
            Grid mainGrid = new Grid();
            mainGrid.Width = this.Width;
            mainGrid.Height = this.Height;
            //mainGrid.ShowGridLines = true;


            // Слева fieldColumn - столбец игрового поля; справа infoColumn - столбец статистики
            ColumnDefinition fieldColumn = new ColumnDefinition();
            fieldColumn.Width = new GridLength(fieldProportion*100, GridUnitType.Star);
            mainGrid.ColumnDefinitions.Add(fieldColumn);

            ColumnDefinition infoColumn = new ColumnDefinition();
            infoColumn.Width = new GridLength(100-fieldProportion*100, GridUnitType.Star);
            mainGrid.ColumnDefinitions.Add(infoColumn);

            // Сетка для элементов infoColumn
            Grid infoGrid = new Grid();

            RowDefinition statusRow = new RowDefinition();
            statusRow.Height = new GridLength(mainGrid.Height/5, GridUnitType.Star);
            infoGrid.RowDefinitions.Add(statusRow);

            RowDefinition moveRow = new RowDefinition();
            moveRow.Height = new GridLength(mainGrid.Height / 10, GridUnitType.Star);
            infoGrid.RowDefinitions.Add(moveRow);

            RowDefinition controlRow = new RowDefinition();
            controlRow.Height = new GridLength(7*mainGrid.Height / 10, GridUnitType.Star);
            infoGrid.RowDefinitions.Add(controlRow);

            Grid.SetColumn(infoGrid, 1);
            //infoGrid.ShowGridLines = true;


            // Фоновое изображение в infoColumn
            Image bg = new Image();
            bg.Source = bgBitmap;
            Grid.SetColumn(bg, 1);
            Grid.SetRow(bg, 0);
            mainGrid.Children.Add(bg);

            // Надпись победы/поражения/игры
            victory.Source = victoryBitmap;
            victory.VerticalAlignment = VerticalAlignment.Center;
            victory.HorizontalAlignment = HorizontalAlignment.Center;
            victory.Visibility = Visibility.Hidden;
            Grid.SetRow(victory, 0);
            infoGrid.Children.Add(victory);

            defeat.Source = defeatBitmap;
            defeat.VerticalAlignment = VerticalAlignment.Center;
            defeat.HorizontalAlignment = HorizontalAlignment.Center;
            defeat.Visibility = Visibility.Hidden;
            Grid.SetRow(defeat, 0);
            infoGrid.Children.Add(defeat);

            run.Source = runBitmap;
            run.VerticalAlignment = VerticalAlignment.Center;
            run.HorizontalAlignment = HorizontalAlignment.Center;
            run.Visibility = Visibility.Visible;
            Grid.SetRow(defeat, 0);
            infoGrid.Children.Add(run);

            // Картинка с управлением
            Image controls = new Image();
            controls.Source = controlsBitmap;
            controls.VerticalAlignment = VerticalAlignment.Center;
            controls.HorizontalAlignment = HorizontalAlignment.Center;
            Grid.SetRow(controls, 2);
            infoGrid.Children.Add(controls);


            // Счетчик ходов в infoColumn
            tbMove.Text = $"Moves: {moveCount}";

            tbMove.HorizontalAlignment = HorizontalAlignment.Center;
            tbMove.VerticalAlignment = VerticalAlignment.Center;

            tbMove.FontFamily = new FontFamily("Georgia");
            tbMove.FontSize = 38;
            tbMove.Foreground = Brushes.AntiqueWhite;

            Grid.SetRow(tbMove, 1);
            infoGrid.Children.Add(tbMove);

            mainGrid.Children.Add(infoGrid);


            // Сетка с клетками поля (помещается в fieldColumn)
            fieldGrid.Width = mainGrid.Width*fieldProportion;
            //fieldGrid.ShowGridLines = false;
            for (int i = 0; i < fieldN; i++)
            {
                ColumnDefinition colDef = new ColumnDefinition();
                fieldGrid.ColumnDefinitions.Add(colDef);

                RowDefinition rowDef = new RowDefinition();
                fieldGrid.RowDefinitions.Add(rowDef);
            }

            Grid.SetColumn(fieldGrid, 0);
            Grid.SetRow(fieldGrid,0);

            // Обновить лабиринт
            ResetLab(); 

            // Заполнение поля изображениями
            Image filler = new Image();
            filler.Source = blankBitmap;
            Grid.SetColumn(filler, 0);
            mainGrid.Children.Add(filler);
            RedrawField();



            mainGrid.Children.Add(fieldGrid);


            this.Content = mainGrid;

        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W: { wReady = true; break; }
                case Key.A: { aReady = true; break; }
                case Key.S: { sReady = true; break; }
                case Key.D: { dReady = true; break; }
                case Key.R: { rReady = true; break; }
                case Key.N: { nReady = true; break; }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

            int di = 0;
            int dj = 0;

            switch (e.Key)
            {
                case Key.W: { if (!wReady || blocked) return; wReady = false; di = -1; dj = 0; break; }
                case Key.A: { if (!aReady || blocked) return; aReady = false; di = 0; dj = -1; break; }
                case Key.S: { if (!sReady || blocked) return; sReady = false; di = +1; dj = 0; break; }
                case Key.D: { if (!dReady || blocked) return; dReady = false; di = 0; dj = +1; break; }
                case Key.N: { if (!nReady) return; nReady = false; ResetLab(); RedrawAll(); blocked = false; break; }
                case Key.R: { 

                        if (!rReady) return; rReady = false;

                        fieldArr[iHero, jHero] = blankN;
                        iHero = startI; jHero = startJ;
                        fieldArr[iHero, jHero] = heroN;

                        fieldArr[exitI, exitJ] = exitN;

                        moveCount = 0;

                        RedrawAll();
                        blocked = false; 
                        break; 

                    }
                case Key.Escape: { Application.Current.Shutdown(); break; }
            }

            
            switch (CheckMove(iHero + di, jHero + dj))
            {
                case 0: {

                        fieldArr[iHero, jHero] = blankN;
                        SquareRedraw(iHero, jHero);


                        iHero += di;
                        jHero += dj;

                        fieldArr[iHero, jHero] = heroN;
                        SquareRedraw(iHero, jHero);

                        moveCount++;

                        tbMove.Text = $"Moves: {moveCount}";

                        break; 
                    }
                case 1: {

                        fieldArr[iHero, jHero] = blankN;
                        SquareRedraw(iHero, jHero);


                        iHero += di;
                        jHero += dj;

                        fieldArr[iHero, jHero] = victN;
                        SquareRedraw(iHero, jHero);

                        moveCount++;

                        tbMove.Text = $"in {moveCount} moves. Press N to start new level";
                        victory.Visibility = Visibility.Visible;
                        run.Visibility = Visibility.Hidden;

                        blocked = true;

                        break;
                    }
                case 2: {

                        fieldArr[iHero, jHero] = defN;
                        SquareRedraw(iHero, jHero);

                        moveCount++;

                        tbMove.Text = $"in {moveCount} moves. Press R to retry";

                        defeat.Visibility = Visibility.Visible;
                        run.Visibility = Visibility.Hidden;

                        blocked = true;
                        break;
                    }
            }

        }

    }


}