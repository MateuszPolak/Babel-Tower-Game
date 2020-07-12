using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BabelClient
{
    public partial class MainWindow : Window
    {
        int player_turn = 0;
        int max_player = 1;
        double speed = -5;
        double speed2 = -5;
        double turbo = 0;
        double TopBlock = 0;
        string active_player = "";
        int isTimerRuning = 0; // 0= no, 1= yes
        Grid GameCanvas = new Grid();
        public static int direction = 0; // 0 = left/ right, 1 = down
        List<Canvas> AllBlocksList = new List<Canvas>();
        List<Canvas> DroppedBlocksList = new List<Canvas>();
        List<string> Players = new List<string>();
        Random rand = new Random();


        public MainWindow()
        {
            InitializeComponent();
            labelGOV.Visibility = Visibility.Hidden;
            button_drop_test.Visibility = Visibility.Hidden;
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            string suma = "";
            int test = Canvas1.Children.OfType<UIElement>().Count();
            for (int q2 = 0; q2 < test; q2++)
            {
                suma = suma + " " + Canvas1.Children[q2];
            }
            int all = AllBlocksList.Count();
            int all2 = DroppedBlocksList.Count();
            int i;
            for (i = all - 1; i<all; i++)
            {
                var ActiveBlock = AllBlocksList[i];
                string cont = ActiveBlock.Uid;
                if (direction == 0)
                {
                    ActiveBlock.Margin = new Thickness(ActiveBlock.Margin.Left - speed, ActiveBlock.Margin.Top, 0, 0);
                    if (ActiveBlock.Margin.Left < -BorderPanel.Width + ActiveBlock.Width) //left
                    {
                        ActiveBlock.Margin = new Thickness(-BorderPanel.Width + ActiveBlock.Width, ActiveBlock.Margin.Top, 0, 0);
                        speed = -speed;
                    }
                    if (ActiveBlock.Margin.Left + ActiveBlock.Width > BorderPanel.Width) //right
                    {
                        ActiveBlock.Margin = new Thickness(BorderPanel.Width - ActiveBlock.Width, ActiveBlock.Margin.Top, 0, 0);
                        speed = -speed;
                    }
                    if (TopBlock < 100)
                    {
                        if (TopBlock < 1)
                        {
                            TopBlock = 100;
                        }
                        foreach (Canvas recdrop in DroppedBlocksList)
                        {
                            recdrop.Margin = new Thickness(recdrop.Margin.Left, recdrop.Margin.Top + 10, 0, 0);
                            TopBlock = DroppedBlocksList[all2 - 1].Margin.Top;
                            string content = "";
                            double pos = recdrop.Margin.Top;
                            content = content + " " + pos;
                        }
                    }
                    string s222 = all + " " + all2;
                }
                else if (direction == 1) //down
                {
                    speed = speed2;
                    ActiveBlock.Margin = new Thickness(ActiveBlock.Margin.Left, ActiveBlock.Margin.Top - speed, 0, 0);
                    if (ActiveBlock.Margin.Top + ActiveBlock.Height*2 >= TopBlock) //down to upper block
                    {
                        ActiveBlock.Margin = new Thickness(ActiveBlock.Margin.Left,TopBlock-ActiveBlock.Height*2, 0, 0);
                        TopBlock = ActiveBlock.Margin.Top;
                        if (all2>0)
                        {
                            double av = ActiveBlock.Margin.Left;
                            int last_n = all2-1;
                            if (last_n > 3)
                            {
                                last_n = 4;
                            }
                            double cntr_mass = 0;
                            for (int d = 0; d < (last_n+1); d++)
                            {

                                cntr_mass = av/(d+1);
                                double com_difr = Math.Abs(cntr_mass - DroppedBlocksList[all2 - 1 - d].Margin.Left);
                                if ( com_difr > ActiveBlock.Width)
                                {
                                    
                                    labelGOV.Visibility = Visibility.Visible;
                                    ActiveBlock.Background = Brushes.Red;
                                    speed = 0;
                                    speed2 = 0;
                                    buttonPlay.Content = "Play";
                                    labelGOV.Content = "GAME OVER\nplayer " + active_player + "\nLOST!";
                                    buttonPlay.Visibility = Visibility.Visible;

                                }
                                av += DroppedBlocksList[all2 -1 - d].Margin.Left;
                            }
                        }
                        DroppedBlocksList.Add(ActiveBlock);
                        if (player_turn == max_player)
                        {
                            active_player = Players[0];
                            player_turn = 0;
                        }
                        else
                        {
                            player_turn += 1;
                            active_player = Players[player_turn];
                        }
                        Activeplayer.Content = "Active player: " + active_player;

                        direction = 0;
                        if (labelGOV.IsVisible == false)
                        {
                            turbo += -0.05;
                            speed += turbo;
                            speed2 += turbo;
                            NewBlock();
                        }
                        
                    }

                }
            }
        }

        void ReceiveData(TcpClient client)
        {
            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;
            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                string  data = Encoding.ASCII.GetString(receivedBytes, 0, byte_count);
                if (data.Contains("drop") == true)
                {
                    string pl = data.Remove(0, 6);
                    if (active_player == pl)
                    {
                        direction = 1;
                    }
                }

                if (data.Contains("player") == true)
                {
                    string IDname = "";
                    IDname = data.Remove(0, 6);
                    string Name = IDname.Remove(0, 1);
                    Players.Add(Name);
                    {
                        labelPL.Dispatcher.Invoke(delegate
                        {
                            string label_players = "";
                            foreach (string strPL in Players)
                            {
                                label_players = label_players +"\n"+strPL;
                            }
                            labelPL.Content = "Players:"+label_players;
                        });
                    }
                }
            }
        }

        private void NewBlock()
        {

            Canvas myCanvas = new Canvas();
            myCanvas.Background = Brushes.Black;
            myCanvas.Height = 50;
            myCanvas.Width = 150;
            int random_x = rand.Next(-320,320);
            myCanvas.Margin = new Thickness(random_x, -400, 0, 0);
            Canvas1.Children.Add(myCanvas);
            AllBlocksList.Add(myCanvas);
            this.Show();
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            //Set IP
            IPAddress ip = IPAddress.Parse("IP");
            int port = 5001;
            TcpClient client = new TcpClient();
            client.Connect(ip, port);
            buttonConnect.Background = Brushes.DarkGreen;
            NetworkStream ns = client.GetStream();
            Thread thread = new Thread(o => ReceiveData((TcpClient)o));
            thread.Start(client);
            string s = "7";
            byte[] buffer = Encoding.ASCII.GetBytes(s);
            ns.Write(buffer, 0, buffer.Length);
        }
        
        private void buttonPlay_Click(object sender, RoutedEventArgs e)
        {
            labelGOV.Visibility = Visibility.Hidden;
            buttonPlay.Visibility = Visibility.Hidden;
            speed = -5;
            speed2 = -5;
            turbo = 0;
            player_turn = 0;
            max_player = Players.Count() - 1;
            if (Players.Count() == 0)
            {
                labelGOV.Content = "No players!\nconnect and try again";
                labelGOV.Visibility = Visibility.Visible;
            }
            else
            {
                active_player = Players[0];
                Activeplayer.Content = "Active player: " + active_player;
                string suma = "";
                int test = Canvas1.Children.OfType<UIElement>().Count();
                for (int q2 = 0; q2 < test; q2++)
                {
                    suma = suma + " " + Canvas1.Children[q2];
                }
                int all = AllBlocksList.Count();
                int nottoremoverange = 8;
                Canvas1.Children.RemoveRange(nottoremoverange, all);
                AllBlocksList.Clear();
                DroppedBlocksList.Clear();
                NewBlock();
                TopBlock = BorderPanel.Height + 50;
                labelGOV.Visibility = Visibility.Hidden;
                if (isTimerRuning == 0)
                {
                    runtimer();
                }
            }
        }

        private void button_drop_test_Click(object sender, RoutedEventArgs e)
        {
            direction = 1;
        }

        private void runtimer()
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new
            System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(50, 0, 0);
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(1);
            dispatcherTimer.Start();
            isTimerRuning = 1;
        }
    }
}