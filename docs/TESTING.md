# Provare il driver in sicurezza

Il driver non conosce nessun comando finche' non li inserisci tu nel wizard. Per provare una nuova build
senza rischi per la cupola:

## 1. Test automatici (nessuna scheda coinvolta)

```
Tests/ClamshellCore.Tests/build_and_run.sh      # richiede Mono (mcs)
```

Girano contro un server HTTP finto su `127.0.0.1`. Impostano `CLAMSHELL_TEST_MODE=1`: con questa variabile
il driver **rifiuta qualsiasi indirizzo che non sia loopback**, quindi un test non puo' raggiungere la scheda vera.

## 2. Prima prova in NINA: modalita' simulazione

1. Nel setup spunta **Simulation mode** (scheda *Options*). Nessuna richiesta di rete viene inviata: i comandi
   compaiono solo nel log (`SIMULATED GET ...`).
2. Inserisci comandi e tempi di corsa di prova, collega la cupola da NINA e prova Apri/Chiudi:
   lo stato deve passare da `Opening` a `Open` dopo il tempo di corsa.
3. Il log del driver e' in `Documents\ASCOM\Logs <data>\ASCOM.Clamshell.Hardware...`.

## 3. Prova su rete locale senza la scheda

Con la variabile d'ambiente `CLAMSHELL_TEST_MODE=1` impostata **prima di avviare NINA**, il driver parla solo con
`127.0.0.1`. Avvia un piccolo server di prova su quella macchina e punta il driver li'.

## 4. Prova reale

Solo dopo i passi precedenti, togli la simulazione e invia il primo comando con la cupola sotto controllo visivo.
Se un comando fallisce lo stato diventa `Error`: controlla la cupola a occhio e, se serve, riallinea il driver
con l'azione ASCOM `SetAssumedState` (`open` o `closed`).

## Nota sull'installazione

Questa versione ha lo stesso ProgID (`ASCOM.Clamshell.Dome`) e lo stesso GUID di quella funzionante: registrarla
(`ASCOM.Clamshell.exe /register`) **sostituisce** la registrazione esistente. Per provarla in parallelo cambia
`ProgId` e `Guid` in `DomeDriver.cs` prima di compilare una build di prova.
