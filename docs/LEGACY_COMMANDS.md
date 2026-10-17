# Valori della vecchia versione (solo riferimento)

Queste sono le impostazioni che la versione precedente del driver aveva scritte nel codice.
**Non sono usate dal driver**: vanno inserite a mano nel wizard di setup, solo se la tua scheda
di rele' IP e' la stessa. Una scheda diversa avra' percorso e numeri diversi.

| Azione  | Richieste HTTP GET (un impulso ciascuna, inviate in parallelo) |
|---------|-----------------------------------------------------------------|
| Apertura | `http://<IP scheda>/protect/leds.cgi?led=5` e `...led=3`        |
| Chiusura | `http://<IP scheda>/protect/leds.cgi?led=4` e `...led=2`        |

Nel wizard corrispondono a:

- Modello percorso: `/protect/leds.cgi?led={relay}`
- Rele' apertura: `5, 3`
- Rele' chiusura: `4, 2`
- Esecuzione: in parallelo
- Autenticazione: Basic (utente e password della scheda)

Username e password **non** sono riportati qui. Restano nel profilo ASCOM della versione vecchia;
alla prima apertura del nuovo setup vengono migrati (la password viene cifrata).
