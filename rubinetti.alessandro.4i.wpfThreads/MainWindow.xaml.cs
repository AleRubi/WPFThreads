using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace rubinetti.alessandro._4i.wpfThreads
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int GIRI1 = 500;
        const int GIRI2 = 1000;
        int _counter = 0;
        static readonly object _locker = new object();

        CountdownEvent semaforo;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            btnGo.IsEnabled = false;

            Thread thread1 = new Thread(incrementa1);
            thread1.Start();

            Thread thread2 = new Thread(incrementa2);
            thread2.Start();

            semaforo = new CountdownEvent(2);

            Thread thread3 = new Thread( () =>
                {
                    semaforo.Wait();
                    Dispatcher.Invoke(() =>
                        {
                            lblCounter1.Text = _counter.ToString();
                            lblCounter2.Text = _counter.ToString();
                            btnGo.IsEnabled = true;
                        }
                    );
                }
            );
            thread3.Start();
        }

        private void incrementa1()
        {
            for (int x = 0; x < GIRI1; x++)
            {
                lock (_locker)
                {
                    _counter++;
                }

                Dispatcher.Invoke(

                    () =>

                    {
                        lblCounter1.Text = _counter.ToString();
                    }

                );

                Thread.Sleep(1);
            }
        }

        private void incrementa2()
        {
            for (int x = 0; x < GIRI2; x++)
            {
                lock (_locker)
                {
                    _counter++;
                }

                Dispatcher.Invoke(

                    () =>

                    {
                        lblCounter2.Text = _counter.ToString();
                    }

                );

                Thread.Sleep(1);
            }
        }
    }
}
