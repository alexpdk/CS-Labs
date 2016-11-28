using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DrugAccount {
	public interface IExceptionLogger {
		void LogDrugAccountException(DrugAccountException e);
		void LogSystemException(Exception e);
		void StopLogging();
	}
	public abstract class ExceptionLogger : IExceptionLogger {
		protected TextWriter writer;

		public void LogDrugAccountException(DrugAccountException e) {
			writer.WriteLine("\nDrug Account Exception: {0}\n{1}",e.Message,e.StackTrace);
		}
		public void LogSystemException(Exception e) {
			writer.Write("\n{0} ", DateTime.Now);
			writer.WriteLine("{0}: {1}\n{2}",e.GetType(),e.Message,e.StackTrace);
		}
		public virtual void StopLogging() {
			writer.Close();
		}
	}
	public class ConsoleExceptionLogger : ExceptionLogger {
		public ConsoleExceptionLogger(){
			writer = Console.Out;
		}
	}
	public class FileExceptionLogger : ExceptionLogger {
		public FileExceptionLogger(string path){
			if(!File.Exists(path)) {
				Debug.Write("Create file");
				var stream = File.Create(path);
				writer = new StreamWriter(stream);
			}else {
				writer=File.AppendText(path);
				Debug.Write("File exists");
			}
		}
	}
}
