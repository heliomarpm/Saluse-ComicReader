using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Diagnostics;

namespace Saluse.ComicReader.Application.Managers
{
	class SpeechManager
	{
		private string[] CommandList =
		{
			"Zoom",
			"Next",
			"Back",
			"Window",
			"Rotate",
			"First",
			"Last",
			"Info"
		};

		private SpeechRecognitionEngine _speechRecognitionEngine;
		private bool _isRunning = false;

		public Action<string, string> CommandRecognized = null;

		public SpeechManager()
		{
			_speechRecognitionEngine = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-GB"));

			BuildGrammar();

			BindEvents();

			// Don't start recognising on start
			//InitialiseRecognitionEngine();
		}

		private void BuildGrammar()
		{
			var choices = new Choices(CommandList);
			var generalGrammarBuilder = new GrammarBuilder(choices);
			var generalGrammar = new Grammar(generalGrammarBuilder);
			generalGrammar.Name = "General";
			_speechRecognitionEngine.LoadGrammar(generalGrammar);

			var gotoPageGrammarBuilder = new GrammarBuilder();
			gotoPageGrammarBuilder.Append("Page");
			var pageNumbers = Enumerable.Range(1, 100).Select(x => x.ToString()).ToArray();
			gotoPageGrammarBuilder.Append(new Choices(pageNumbers));

			var pageGrammar = new Grammar(gotoPageGrammarBuilder);
			pageGrammar.Name = "PageGoto";
			_speechRecognitionEngine.LoadGrammar(pageGrammar);

			////see: https://stackoverflow.com/questions/18821566/accuracy-of-ms-system-speech-recognizer-and-the-speechrecognitionengine
			//DictationGrammar dictationGrammar = new DictationGrammar("grammar:dictation#pronunciation");
			//dictationGrammar.Name = "Random";
			//_speechRecognitionEngine.LoadGrammar(dictationGrammar);
		}

		private void BindEvents()
		{
			_speechRecognitionEngine.SpeechRecognized += OnSpeechRecognized;
			_speechRecognitionEngine.RecognizeCompleted += OnRecognizeCompleted;
		}

		private void InitialiseRecognitionEngine()
		{
			_speechRecognitionEngine.SetInputToDefaultAudioDevice();
			_speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
			_isRunning = true;
		}

		private void OnSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
		{
			if (this.CommandRecognized != null)
			{
				var grammarName = e.Result.Grammar.Name.ToLower();
				////see: https://stackoverflow.com/questions/18821566/accuracy-of-ms-system-speech-recognizer-and-the-speechrecognitionengine
				//if (grammarName != "random")
				if (e.Result.Confidence > 0.95)
				{
					Debug.WriteLine($"{DateTime.Now} - '{e.Result.Grammar.Name}': '{e.Result.Text}' @ {e.Result.Confidence}");
					switch (grammarName)
					{
						case "general":
							this.CommandRecognized(e.Result.Grammar.Name, e.Result.Text.ToLower());
							break;

						case "pagegoto":
							this.CommandRecognized(e.Result.Grammar.Name, e.Result.Words[1].Text);
							break;
					}
				}
			}
		}

		private void OnRecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
		{
			if (e.Cancelled)
			{
				_isRunning = false;
			}
		}

		public bool ToogleRecognition()
		{
			if (_isRunning)
			{
				_speechRecognitionEngine.RecognizeAsyncCancel();
				return false;
			}
			else
			{
				InitialiseRecognitionEngine();
				return true;
			}
		}
	}
}
