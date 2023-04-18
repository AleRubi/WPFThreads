# WPFThreads
## Appunti lezione del 09/02/2023
Un thread è un processo
```
<Grid ShowGridLines="True">
        <Grid.RowDefinitions>
            <RowDefinition Height="100"></RowDefinition>
            <RowDefinition></RowDefinition>
        </Grid.RowDefinitions>

        <StackPanel Grid.Row="0" Orientation="Horizontal">
            <Button x:Name="btnGo" Background="Lime" Width="400" Click="Button_Click">START</Button>
            <Button x:Name="btnClear" Width="400" Background="Red" Click="btnClear_Click">RESET</Button>
        </StackPanel>

        <StackPanel Grid.Row="1">
            <TextBlock x:Name="lblCounter1" FontSize="50"></TextBlock>
            <TextBlock x:Name="lblCounter2" FontSize="50"></TextBlock>
            <TextBlock x:Name="lblCounter3" FontSize="50"></TextBlock>
            <TextBlock x:Name="lblCounter4" FontSize="50"></TextBlock>
            <ProgressBar x:Name="prbarCounter1" Height="50" Maximum="0"></ProgressBar>
        </StackPanel>
    </Grid>
```
```
private void Button_Click(object sender, RoutedEventArgs e)
{
            Thread thread1 = new Thread(incrementa1);
            thread1.Start();
}
```
Questo programma lancia un thread
```
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
```
Thread.Sleep(1); fa “dormire” il thread per 1 millisecondi ogni giro

Dispatcher.Invoke() rende il processo non bloccante 
```
=> //lambda expression, indica un blocco di codice da far eseguire al debugger/compilatore
```
## Appunti lezione del 16/02/2023
```
Thread.Sleep(1);
```
Thread.Sleep ti permette di addormentare il sistema per un tot di millisecondi.

Quando avviamo il programma non riusciamo a fare niente questo perché esso va in “hang”, il thread della grafica si blocca, perché è un processo bloccante. 
Dobbiamo fare in modo che questo processo lento lasci stare il thread dell’interfaccia grafica, per far ciò creiamo un nuovo thread sul metodo incrementa e facciamo partire il processo

```
private void incrementa()
{
    for (int x = 0; x < GIRI1; x++)
    {
            _counter++;

        Dispatcher.Invoke(() => {
            lblCounter.Text = _counter.ToString();
        });
        Thread.Sleep(100);
    }
}
```
Dispatcher.Invoke() abbiamo il lasciapassare e facciamo robe che normalmente non potremmo fare 
```
public partial class MainWindow : Window
{
    const int GIRI = 1000;
    int _counter = 0;
    static readonly object _locker = new object();
}
```
Creaiamo un oggetto _locker 
```
lock (_locker)
{
    _counter++;
}
```
Dentro al lock, questa è la cosiddetta zona critica
## Appunti lezione del 02/03/2023
Perché i due thread segnano due valori diversi? Inizialmente proviamo a diminuire i giri di uno dei cicli, ci accorgiamo che sulla finestra uno dei due non ha il valore giusto, quindi questa soluzione non va bene. Ci serve un meccanismo che aspetta che finisca uno, che aspetta che finisca anche l’altro e poi successivamente fa quello che deve fare. Questo possiamo farlo con un semaforo, vengono forniti dal kernel, è uno strumento di basso livello. Il lock garantisce che nessuno interferisca con l’elaborazione di quel thread. Il semaforo è un  intero che non può essere negativo: ha due istruzioni signal() e wait(). Signal diminuisce il contatore e Wait aspetta che questo contatore arrivi a 0.
Per fare ciò, creiamo un oggetto di tipo countdownevent
```
public partial class MainWindow : Window
{
    const int GIRI1 = 1000;
    const int GIRI1 = 50;
    
    int _counter = 0;
    static readonly object _locker = new object();
    
    CountdownEvent semaforo;
    
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Thread thread1 = new Thread(incrementa1);
        thread1.Start();

        Thread thread2 = new Thread(incrementa2);
        thread2.Start();

        semaforo = new CountdownEvent(2);
}
```
poi dobbiamo fare:
```
semaforo.Wait();
```
Ora adesso servono le istruzioni per portare a zero il countdown e per fare ciò alla fine di ogni Thread metteremo questo comando:
```
semaforo.Signal();
```
Notiamo un problema quando: 
il semaforo non può essere allegramente usato all’interno degli handler

Creiamo un nuovo thread 
```
Thread thread3 = new Thread(attendi);
```
al posto di attendi possiamo anche inserirli dentro tutto il codice
```
private void attendi()
{
    semaforo.Wait();

    Dispatcher.Invoke(() => {
        lblCounter1.Text = _counter.ToString();
        lblCounter2.Text = _counter.ToString();
    });
    
}
```
In questo modo alla fine nella finestra vedremo lo stesso risultato però con una certa violenza
```
Thread thread3 = new Thread(() =>
{
    semaforo.Wait();
    Dispatcher.Invoke(() =>
    {
         lblCounter1.Text = _counter.ToString();
         lblCounter2.Text = _counter.ToString();
    }
    );
}
);

thread3.Start();
```
Il thread3 inizializza questo codice e poi parte

Se riclicco il bottone succede che spacco il semaforo e di conseguenza spacco tutto il codice

Bisogna fare in modo che quando io per la prima volta clicco il bottone esso venga bloccato fino a quando esso non ha finito di svolgere tutte le sue operazioni, da mettere nel metodo Button_Clicked()
```
// Spegne momentaneamente il pulsante
btnGo.IsEnabled = false;
```
Così però esso fa solo un giro quindi lo riaccendiamo alla fine del Dispatcher.Invoke() nel thread3
```
Thread thread3 = new Thread(() =>
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
```
## Diario WPFThreads
### Inizializzare un thread
```
Thread thread1 = new Thread(incrementa1);
thread1.Start();
```
Queste due righe di codice creano un nuovo oggetto di tipo Thread chiamato "thread1" e gli assegnano come parametro un metodo chiamato "incrementa1". Successivamente, il metodo "Start" viene chiamato sul thread appena creato per avviare l'esecuzione del metodo "incrementa1". In questo modo è possibile eseguire operazioni il parallelo rispetto al thread principale del programma, questo però può portare a problemi di sincronizzazione e concorrenza tra thread.
### Il costrutto lock()
```
static readonly object _locker = new object();
lock (_locker)
{
 _counter++;
}
```
Iniziamo dichiarando una variabile statica chiamata "_locker" e gli assegniamo il tipo object, questo ci servirà come meccanismo di sincronizzazione tra i thread. Successivamente grazie a “lock” incrementiamo il nostro contatore, questo impedisce ad altri thread di accedere al contatore, garantendo che solo il thread in questione posso accedere al contatore. Questo meccanismo è utilizzato per evitare problemi di concorrenza quando più thread tentano di accedere o modificare la stessa variabile o risorsa allo stesso tempo.
### Utilizzo di un semaforo
```
CountdownEvent semaforo = new CountdownEvent(3);
semaforo.Wait();
semaforo.Signal();
```
Il semaforo "CountdownEvent" può essere utilizzato per sincronizzare l'esecuzione tra più thread, attendendo che un determinato numero di thread eseguano determinate operazioni prima di permettere ad altri thread di accedere alle risorse condivise. Il metodo “Wait” sospende il thread corrente fino a che esso non diventa 0, mentre “Signal” decrementa il semaforo di 1. Quando il contatore raggiunge lo zero, il semaforo segnala che il conto alla rovescia è terminato e tutti i thread in attesa sul metodo "Wait" vengono sbloccati e possono continuare l'esecuzione.
### La lambda expression
Una lambda expression è una sintassi breve e concisa per definire una funzione senza dover scrivere una classe separata o un metodo esplicito.
```
(int x) => x * 2
```
Ad esempio, la seguente lambda expression definisce una funzione che prende in input un intero e restituisce il doppio del valore:
### Dispatcher.Invoke()
```
Dispatcher.Invoke(() => {
   ...
});
```
Il metodo "Invoke" del Dispatcher viene utilizzato per eseguire un'azione su un thread dell'interfaccia utente. Il Dispatcher è un oggetto che gestisce la coda di elaborazione dei thread dell'interfaccia utente in un'applicazione WPF, garantisce inoltre che tutti gli accessi ai controlli dell'interfaccia utente avvengano su un singolo thread così da evitare problemi di sincronizzazione e di accesso concorrente. Il metodo "Invoke" del Dispatcher viene chiamato sul Dispatcher corrente e richiede un oggetto come parametro che deve essere eseguita sul thread dell'interfaccia utente.

Inizio:
![result](https://github.com/AleRubi/WPFThreads/blob/main/Img/Inizio.png?raw=true)

Esecuzione in corso:
![result](https://github.com/AleRubi/WPFThreads/blob/main/Img/Intermezzo.png?raw=true)

Fine:
![result](https://github.com/AleRubi/WPFThreads/blob/main/Img/Fine.png?raw=true)
