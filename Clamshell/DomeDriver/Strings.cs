using System;
using System.Collections.Generic;
using System.Globalization;

namespace ASCOM.Clamshell.Dome
{
    /// <summary>
    /// Localised texts for the setup dialog. Italian is used when the current UI culture is Italian,
    /// English otherwise. Missing keys fall back to English and finally to the key itself.
    /// </summary>
    public static class Strings
    {
        /// <summary>Project page opened from the Options tab.</summary>
        public const string DocsUrl = "https://github.com/RobertPanai/ascom-driver-for-ip-clamshell-dome";

        /// <summary>Official ASCOM standards site opened by clicking the ASCOM logo.</summary>
        public const string AscomUrl = "https://ascom-standards.org/";

        private static readonly Dictionary<string, string> English = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> Italian = new Dictionary<string, string>();

        static Strings()
        {
            // ---- header and generic ----
            Add("title.form", "Clamshell Setup", "Configurazione Clamshell");
            Add("header.title", "Clamshell IP Relay Dome", "Clamshell IP Relay Dome");
            Add("header.version", "Version {0}", "Versione {0}");
            Add("header.author", "by Robert Panai", "di Robert Panai");
            Add("tip.ascom", "Open ascom-standards.org", "Apri ascom-standards.org");
            Add("msg.error", "Error", "Errore");
            Add("AlreadyConnected", "The dome is connected: disconnect it from the client application before changing the settings.",
                                     "La cupola è connessa: disconnettila dall'applicazione prima di modificare le impostazioni.");
            Add("msg.not_valid", "The configuration is not valid: fix the items shown in red.",
                                 "La configurazione non è valida: correggi gli elementi in rosso.");
            Add("msg.ascom_fail", "Could not open the browser: {0}", "Impossibile aprire il browser: {0}");

            // ---- wizard buttons ----
            Add("btn.back", "< Back", "< Indietro");
            Add("btn.next", "Next >", "Avanti >");
            Add("btn.ok", "OK", "OK");
            Add("btn.cancel", "Cancel", "Annulla");

            // ---- tabs ----
            Add("tab.connection", "1. Connection", "1. Connessione");
            Add("tab.commands", "2. Commands", "2. Comandi");
            Add("tab.timing", "3. Timing && status", "3. Tempi e stato");
            Add("tab.options", "4. Options", "4. Opzioni");

            // ---- status strip ----
            Add("status.ready", "Ready - the configuration is valid.", "Pronto - la configurazione è valida.");
            Add("status.sim_ready", "Simulation mode - the real dome will NOT be commanded.",
                                    "Modalità simulazione - la cupola reale NON verrà comandata.");
            Add("status.invalid", "Not ready: {0}", "Non pronto: {0}");
            Add("status.first_run", "First setup: fill in the connection and the commands, then press OK.",
                                    "Prima configurazione: compila connessione e comandi, poi premi OK.");

            // ---- tab 1: connection ----
            Add("cue.host", "192.168.x.x", "192.168.x.x");
            Add("conn.host", "Host / IP", "Host / IP");
            Add("conn.host_hint", "Address or name, optionally with port (name:port). Do not type http://.",
                                  "Indirizzo o nome, eventualmente con porta (nome:porta). Non scrivere http://.");
            Add("conn.https", "Use HTTPS", "Usa HTTPS");
            Add("conn.auth", "Authentication", "Autenticazione");
            Add("auth.none", "None", "Nessuna");
            Add("auth.basic", "HTTP Basic", "HTTP Basic");
            Add("conn.user", "Username", "Nome utente");
            Add("conn.pass", "Password", "Password");
            Add("conn.pass_saved", "A password is saved. Leave the box empty to keep it.",
                                   "Una password è salvata. Lascia la casella vuota per mantenerla.");
            Add("conn.pass_none", "No password saved.", "Nessuna password salvata.");
            Add("conn.pass_will_change", "The new password is protected and saved when you press OK.",
                                         "La nuova password viene protetta e salvata quando premi OK.");
            Add("msg.error_detail", "Error: {0}", "Errore: {0}");
            Add("conn.show_pass", "Show", "Mostra");
            Add("conn.test", "Test connection", "Verifica connessione");
            Add("conn.test_note", "Only a TCP connection is tested. No command is sent.",
                                  "Viene verificata solo una connessione TCP. Nessun comando viene inviato.");
            Add("conn.test_running", "Testing...", "Verifica in corso...");
            Add("conn.test_need_host", "Type the host first.", "Inserisci prima l'host.");
            Add("conn.test_ok", "Host reachable (TCP). No command was sent.",
                                "Host raggiungibile (TCP). Nessun comando è stato inviato.");
            Add("conn.test_fail", "Not reachable: {0}", "Non raggiungibile: {0}");

            // ---- tab 2: commands ----
            Add("cmd.quick", "Quick builder", "Generatore rapido");
            Add("cmd.path", "Path template (use {relay})", "Modello percorso (usa {relay})");
            Add("cue.path", "/path?channel={relay}", "/path?channel={relay}");
            Add("cmd.open_relays", "Open relays", "Relè di apertura");
            Add("cmd.close_relays", "Close relays", "Relè di chiusura");
            Add("cue.relays", "relay values, comma separated", "valori dei relè, separati da virgola");
            Add("cmd.parallel", "Send all steps at the same time", "Invia tutti i passi insieme");
            Add("cmd.delay", "Pause between steps (ms)", "Pausa tra i passi (ms)");
            Add("cmd.relink", "Regenerate steps from quick builder", "Rigenera i passi dal generatore rapido");
            Add("cmd.linked", "The steps are generated from the quick builder.",
                              "I passi sono generati dal generatore rapido.");
            Add("cmd.unlinked", "The steps were edited by hand, so the quick builder is not linked. Press the button above to overwrite them.",
                                "I passi sono stati modificati a mano, quindi il generatore rapido non è collegato. Premi il pulsante sopra per sovrascriverli.");
            Add("cmd.advanced", "Advanced: edit steps", "Avanzate: modifica i passi");
            Add("cmd.action", "Action", "Azione");
            Add("cmd.action_open", "Open", "Apertura");
            Add("cmd.action_close", "Close", "Chiusura");
            Add("cmd.adv_hint", "Each step is one HTTP GET. Placeholders: {scheme} and {host}. The pause is applied after each step (sequential mode only).",
                                "Ogni passo è una richiesta HTTP GET. Segnaposto: {scheme} e {host}. La pausa viene applicata dopo ogni passo (solo in modalità sequenziale).");
            Add("cmd.col_url", "URL template", "Modello URL");
            Add("cmd.col_delay", "Pause after (ms)", "Pausa dopo (ms)");
            Add("cmd.preview_title", "Preview - nothing is sent", "Anteprima - non viene inviato nulla");
            Add("cmd.preview_empty", "(no steps defined)", "(nessun passo definito)");
            Add("cmd.preview_nohost", "(type the host on the Connection tab to see the URLs)",
                                      "(inserisci l'host nella scheda Connessione per vedere gli URL)");
            Add("cmd.preview_line", "{0}. {1}", "{0}. {1}");
            Add("cmd.open_label", "Open", "Apertura");
            Add("cmd.close_label", "Close", "Chiusura");
            Add("cmd.mode_parallel", "all steps at once", "tutti i passi insieme");
            Add("cmd.mode_sequential", "one step after the other", "un passo dopo l'altro");
            Add("cmd.relink_confirm", "Overwrite the steps edited by hand with the values of the quick builder?",
                                      "Sovrascrivere i passi modificati a mano con i valori del generatore rapido?");

            // ---- tab 3: timing and status ----
            Add("time.open", "Open travel time (s)", "Tempo di corsa in apertura (s)");
            Add("time.close", "Close travel time (s)", "Tempo di corsa in chiusura (s)");
            Add("time.timeout", "Request timeout (ms)", "Timeout richiesta (ms)");
            Add("time.regex", "Expected response (regex, optional)", "Risposta attesa (regex, opzionale)");
            Add("cue.regex", "empty = any 2xx response accepted", "vuoto = accetta qualsiasi risposta 2xx");
            Add("time.assumed", "State assumed at startup", "Stato presunto all'avvio");
            Add("time.state_closed", "Closed", "Chiusa");
            Add("time.state_open", "Open", "Aperta");
            Add("time.explain", "The board has NO feedback: the dome state is ESTIMATED from the travel times above, "
                              + "so measure them with a stopwatch. The state is kept after restarts. "
                              + "Use Simulation mode on the Options tab to test without moving the dome.",
                                "La scheda NON ha feedback: lo stato della cupola è STIMATO dai tempi di corsa qui sopra, "
                              + "quindi misurali con un cronometro. Lo stato viene mantenuto dopo i riavvii. "
                              + "Usa la modalità simulazione nella scheda Opzioni per provare senza muovere la cupola.");

            // ---- tab 4: options ----
            Add("opt.sim", "Simulation mode (no network: commands are only logged)",
                           "Modalità simulazione (nessuna rete: i comandi vengono solo registrati)");
            Add("opt.sim_warn", "WARNING: while simulation is on, the real dome is NOT commanded. Turn it off for real use.",
                                "ATTENZIONE: con la simulazione attiva la cupola reale NON viene comandata. Disattivala per l'uso reale.");
            Add("opt.trace", "Enable trace log", "Abilita il log di traccia");
            Add("opt.import", "Import JSON...", "Importa JSON...");
            Add("opt.export", "Export JSON...", "Esporta JSON...");
            Add("opt.export_pass_q", "Include the encrypted password in the exported file?\n\n"
                                   + "It can only be decrypted by this Windows user on this PC.\n\n"
                                   + "Default: No.",
                                     "Includere la password cifrata nel file esportato?\n\n"
                                   + "Può essere decifrata solo da questo utente Windows su questo PC.\n\n"
                                   + "Predefinito: No.");
            Add("opt.import_done", "Configuration imported. Check the tabs, then press OK to apply it.",
                                   "Configurazione importata. Controlla le schede, poi premi OK per applicarla.");
            Add("opt.export_done", "Configuration exported.", "Configurazione esportata.");
            Add("opt.import_fail", "Import failed: {0}", "Importazione non riuscita: {0}");
            Add("opt.export_fail", "Export failed: {0}", "Esportazione non riuscita: {0}");
            Add("opt.filter", "JSON files (*.json)|*.json|All files (*.*)|*.*",
                              "File JSON (*.json)|*.json|Tutti i file (*.*)|*.*");
            Add("opt.docs", "Project documentation", "Documentazione del progetto");
        }

        /// <summary>True when the current UI culture is Italian.</summary>
        public static bool IsItalian
        {
            get
            {
                try
                {
                    return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "it";
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>Returns the localised text for a key.</summary>
        /// <param name="key">Text key.</param>
        public static string Get(string key)
        {
            string value;
            if (IsItalian && Italian.TryGetValue(key, out value))
                return value;
            if (English.TryGetValue(key, out value))
                return value;
            return key;
        }

        /// <summary>Returns the localised text for a key, formatted with string.Format.</summary>
        /// <param name="key">Text key.</param>
        /// <param name="args">Format arguments.</param>
        public static string Format(string key, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, Get(key), args);
        }

        private static void Add(string key, string en, string it)
        {
            English[key] = en;
            Italian[key] = it;
        }
    }
}
