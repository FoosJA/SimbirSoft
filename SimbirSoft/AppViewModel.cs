using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Net.Http;
using System.IO;

namespace SimbirSoft
{
	class AppViewModel
	{
		private string _logPath = Directory.GetCurrentDirectory()+"\\log.txt";
		private string _url = "https://www.simbirsoft.com/";
		public String Url
		{
			get { return _url; }
			set { _url = value; RaisePropertyChanged(); }
		}
		private ObservableCollection<WordCount> _words = new ObservableCollection<WordCount>();
		public ObservableCollection<WordCount> WordList
		{
			get { return _words; }
			set { _words = value; RaisePropertyChanged(); }
		}
		
		public ICommand StartCommand { get { return new RelayCommand(StartExecute, CanStart); } }
		bool CanStart() { return true; }
		void StartExecute()
		{
			try
			{
				using (HttpClientHandler hd = new HttpClientHandler())
				{
					using (var clnt = new HttpClient(hd))
					{
						using (HttpResponseMessage resp = clnt.GetAsync(Url).Result)
						{
							if (resp.IsSuccessStatusCode)
							{
								var html = resp.Content.ReadAsStringAsync().Result;
								if (!string.IsNullOrEmpty(html))
								{
									var doc = new HtmlAgilityPack.HtmlDocument();
									doc.LoadHtml(html);
									var bodyChild = doc.DocumentNode.SelectSingleNode(".//body");
									char[] delimiterChars = { ' ', ',', '.', '!', '?', '"', ';', ':', '[', ']', '(', ')', '\n', '\r', '\t', '—', '«', '»' };
									string[] words = bodyChild.InnerText.Split(delimiterChars);
									var wordList = new ObservableCollection<string>();
									foreach (var word in words)
									{
										if (word != String.Empty)
											wordList.Add(word.ToLower());
									}
									IEnumerable<string> distinctWord = wordList.Distinct();
									foreach (var word in distinctWord)
									{
										var t = wordList.Where(x => x == word);
										WordList.Add(new WordCount { Word = word, Count = t.Count() });
									}
								}
							}
							else
							{
								AppendLog($"HTTP-ответ не получен + {Url}");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				AppendLog(ex.Message);
			}			
		}
		/// <summary>
		/// Запись логов в файл
		/// </summary>
		/// <param name="text"></param>
		public void AppendLog(String text)
		{			
			FileStream logFile = null;
			logFile = File.Open(_logPath, File.Exists(_logPath) ? FileMode.Append : FileMode.OpenOrCreate);
			using (StreamWriter fs = new StreamWriter(logFile))
			{
				fs.WriteLine($"Ошибка: {text} {DateTime.Now}");
			};			
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void RaisePropertyChanged([CallerMemberName] string name = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
