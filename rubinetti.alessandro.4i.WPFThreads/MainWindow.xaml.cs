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

namespace rubinetti.alessandro._4i.WPFThreads
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int GIRI1 = 5000;
        const int GIRI2 = 500;
        const int GIRI3 = 50;

        int _counter = 0;
        static readonly object _locker = new object();

        CountdownEvent semaforo;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Spegne momentaneamente il pulsante
            btnGo.IsEnabled = false;
            btnClear.IsEnabled = false;
            prbarCounter1.Maximum += (GIRI1 + GIRI2 + GIRI3);

            Thread thread1 = new Thread(incrementa1);
            thread1.Start();

            Thread thread2 = new Thread(incrementa2);
            thread2.Start();

            Thread thread4 = new Thread(incrementa3);
            thread4.Start();

            semaforo = new CountdownEvent(3);

            Thread thread3 = new Thread(() =>
            {
                semaforo.Wait();
                Dispatcher.Invoke(() =>
                {
                    lblCounter1.Text = GIRI1.ToString();
                    lblCounter2.Text = GIRI2.ToString();
                    lblCounter3.Text = GIRI3.ToString();

                    prbarCounter1.Value = _counter;
                    btnGo.IsEnabled = true;
                    btnClear.IsEnabled = true;
                }
                );
            }
            );

            thread3.Start();

        }

        // Processo lento che dobbiamo lanciare...
        private void incrementa1()
        {
            for (int x = 0; x < GIRI1; x++)
            {
                lock (_locker)
                {
                    _counter++;

                }

                Dispatcher.Invoke(() => {
                    lblCounter1.Text = x.ToString();
                    lblCounter4.Text = _counter.ToString();
                    prbarCounter1.Value = _counter;
                });

                Thread.Sleep(1);
            }
            semaforo.Signal();
        }

        private void incrementa2()
        {
            for (int x = 0; x < GIRI2; x++)
            {
                lock (_locker)
                {
                    _counter++;
                }

                Dispatcher.Invoke(() => {
                    lblCounter2.Text = (x + 1).ToString();
                });

                Thread.Sleep(10);

            }
            semaforo.Signal();
        }

        private void incrementa3()
        {
            for (int x = 0; x < GIRI3; x++)
            {
                lock (_locker)
                {
                    _counter++;
                }

                Dispatcher.Invoke(() => {
                    lblCounter3.Text = (x + 1).ToString();
                });

                Thread.Sleep(100);
            }
            semaforo.Signal();
        }



        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            _counter = 0;
            lblCounter1.Text = _counter.ToString();
            lblCounter2.Text = _counter.ToString();
            lblCounter3.Text = _counter.ToString();

            prbarCounter1.Value = _counter;
            prbarCounter1.Maximum = 0;

        }
    }
}
