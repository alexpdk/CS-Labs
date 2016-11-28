using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DrugAccount {
	public class DrugAccountException : Exception {
		public DrugAccountException(string msg): base(msg) { }

		public DrugAccountException(string format, params object[] args) :
			base(String.Format(format,args)) { }
	}
}
