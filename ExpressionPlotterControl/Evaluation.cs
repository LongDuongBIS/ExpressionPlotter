using System;

namespace ExpressionPlotterControl
{
	/// <summary>
	/// Summary description for Evaluation.
	/// </summary>
	public class Evaluation
	{
		static int iTotalLength;

		public static double Evaluate(string strExpression, string charX, double dvalueX, string charY, double dvalueY)
		{
		
			strExpression=InsertPrecedenceBrackets(strExpression);
			//	RemoveSpaces(strExpression);
			strExpression=strExpression.Trim();
			return EvaluateInternal(strExpression, charX, dvalueX, charY, dvalueY);
		}

		public static double Evaluate(string strExpression, string charX, double dvalueX)
		{
			strExpression=InsertPrecedenceBrackets(strExpression);
			//	RemoveSpaces(strExpression);
			strExpression=strExpression.Trim();
			return EvaluateInternal(strExpression, charX, dvalueX, "Y", 1);
		}
		static bool IsOperator(string character)
		{
			if ( character == "+" || character == "-" || character == "*" 
				|| character == "/" || character == "^" || character == "%" )
				return true;
			return false;

		}

		static double EvaluateInternal(string strExpression, string charX, double dvalueX, string charY, double dvalueY)
		{
			try
			{
				int iLength=0;
				//static int iTotalLength;
				int iStartLength;
				double dOperand;
				double dAnswer=0;
				string strLastOperator, strAngleOperator, strCurrentChar;
    
				int i=0;
				iStartLength = i + 1;
				strLastOperator = "+";
				strCurrentChar = strExpression.Substring(i, 1);
				while ( strCurrentChar != ")" && i < strExpression.Length )
				{
					i = i + 1;
					strCurrentChar = strExpression.Substring(i, 1);
					if ( strCurrentChar == "(" )
					{
						dOperand = Evaluate(strExpression.Substring(i), charX, dvalueX, charY, dvalueY);
						if ( iLength > 0 )
						{
							strAngleOperator = strExpression.Substring(iStartLength, iLength);
							dOperand = DoAngleOperation(dOperand, strAngleOperator);
							if (dOperand.Equals(double.NaN))
							{
								throw new Exception("Expression resulted in NaN");
							}
						}
						i = i + iTotalLength;
						iStartLength = i + 1;
						iLength = -1;
						dAnswer = DoOperation(dAnswer, dOperand, strLastOperator);
					}	
        
					if ( strCurrentChar == "+" || strCurrentChar == "-" 
						|| strCurrentChar == "*" || strCurrentChar == "/" 
						|| strCurrentChar == "^" || strCurrentChar == "%" 
						|| strCurrentChar == ")" )
					{
						if ( iLength > 0 )
						{
							if ( strExpression.Substring(iStartLength, 1) == charX )
								dOperand = dvalueX;
							else if ( strExpression.Substring(iStartLength, 1) == charY )
								dOperand = dvalueY;
							else if ( strExpression.Substring(iStartLength, 2) == "pi" )
								dOperand = Math.PI;
							else
								dOperand = Convert.ToDouble(strExpression.Substring(iStartLength, iLength));

							dAnswer = DoOperation(dAnswer, dOperand, strLastOperator);
						}
						if ( strCurrentChar != ")" )
							strLastOperator = strCurrentChar;
						iStartLength = i + 1;
						iLength = -1;
					}
					iLength = iLength + 1;
				}
				
				iTotalLength = i;
				return dAnswer;
			}
			catch(Exception exp)
			{
				throw new Exception("Evaluate Error", exp);
			}
		}


		static double DoAngleOperation(double dOperand, string strOperator)
		{
			strOperator = strOperator.ToLower();
			switch (strOperator)
			{
				case "abs":
					return Math.Abs(dOperand);
				case "sin":
					return Math.Sin(dOperand);
				case "cos":
					return Math.Cos(dOperand);
				case "tan":
					return Math.Tan(dOperand);
				case "sec":
					return 1.0 / Math.Cos(dOperand);
				case "cosec":
					return 1.0 / Math.Sin(dOperand);
				case "cot":
					return 1.0 / Math.Tan(dOperand);
				case "arcsin":
					return Math.Asin(dOperand);
				case "arccos":
					return Math.Acos(dOperand);
				case "arctan":
					return Math.Atan(dOperand);
				case "exp":
					return Math.Exp(dOperand);
				case "ln":
					return Math.Log(dOperand);
				case "log":
					return Math.Log10(dOperand);
				case "antilog":
					return Math.Pow(10, dOperand);
				case "sqrt":
					return Math.Sqrt(dOperand);
                
				case "sinh":
					return Math.Sinh(dOperand);
				case "cosh":
					return Math.Cosh(dOperand);
				case "tanh":
					return Math.Tanh(dOperand);
				case "arcsinh":
					return Math.Log(dOperand + Math.Sqrt(dOperand * dOperand + 1));
				case "arccosh":
					return Math.Log(dOperand + Math.Sqrt(dOperand * dOperand - 1));
				case "arctanh":
					return Math.Log((1 + dOperand) / (1 - dOperand)) / 2;
				default:
					throw new ArgumentException("InvalidAngleOperatorExeption");
			}
		}

		// returns dOperant1 (op) dOperand2
		static double DoOperation(double dOperand1, double dOperand2, string strOperator)
		{
			switch (strOperator)
			{
				case "+":
					return dOperand1 + dOperand2;
				case "-":
					return dOperand1 - dOperand2; 
				case "*":
					return dOperand1 * dOperand2;
				case "/":
					return dOperand1 / dOperand2;
				case "^":
					return Math.Pow(dOperand1, dOperand2);
				case "%":
					return dOperand1 % dOperand2;
			}
			return 0;
		}

		static string InsertPrecedenceBrackets(string strExpression)
		{
			int i=0,j=0;
			int iBrackets=0;
			bool bReplace=false;
			int iLengthExpression;

			//Precedence for * && /
			i = 1;
			iLengthExpression = strExpression.Length;
			while (i <= iLengthExpression )
			{
				if ( strExpression.Substring(-1+i, 1) == "*" || strExpression.Substring(-1+i, 1) == "/" )
				{
					for(j=i-1;j>0;j--)
					{
						if ( strExpression.Substring(-1+j, 1) == ")" )
							iBrackets = iBrackets + 1;
						if ( strExpression.Substring(-1+j, 1) == "(" )
							iBrackets = iBrackets - 1;
						if ( iBrackets < 0 )
							break;
						if ( iBrackets == 0 )
						{
							if ( strExpression.Substring(-1+j, 1) == "+" || strExpression.Substring(-1+j, 1) == "-" )
							{
								strExpression = strExpression.Substring(-1+1, j) + "(" + strExpression.Substring(-1+j + 1);
								bReplace = true;
								i = i + 1;
								break;
							}
						}
					}
					iBrackets = 0;
					j = i;
					i = i + 1;
					while (bReplace == true)
					{
						j = j + 1;
						if ( strExpression.Substring(-1+j, 1) == "(" )
							iBrackets = iBrackets + 1;
						if ( strExpression.Substring(-1+j, 1) == ")" )
						{
							if ( iBrackets == 0 )
							{
								strExpression = strExpression.Substring(-1+1, j - 1) + ")" + strExpression.Substring(-1+j);
								bReplace = false;
								i = i + 1;
								break;
							}
							else
								iBrackets = iBrackets - 1;
						}

				
            
						if ( strExpression.Substring(-1+j, 1) == "+" || strExpression.Substring(-1+j, 1) == "-" )
						{
							strExpression = strExpression.Substring(-1+1, j - 1) + ")" + strExpression.Substring(-1+j);
							bReplace = false;
							i = i + 1;
							break;
						}    
					}
				}
		
				iLengthExpression = strExpression.Length;
				i = i + 1;
			}


			//Precedence for ^ && %
			i = 1;
			iLengthExpression = strExpression.Length;
			while ( i <= iLengthExpression )
			{
				if ( strExpression.Substring(-1+i, 1) == "^" || strExpression.Substring(-1+i, 1) == "%" )
				{
					for(j=i-1;j>0;j--)
					{
						if ( strExpression.Substring(-1+j, 1) == ")" )
							iBrackets = iBrackets + 1;
						if ( strExpression.Substring(-1+j, 1) == "(" )
							iBrackets = iBrackets - 1;
						if ( iBrackets < 0 )
							break;
						if ( iBrackets == 0 )
						{
							if ( strExpression.Substring(-1+j, 1) == "+" 
								|| strExpression.Substring(-1+j, 1) == "-" 
								|| strExpression.Substring(-1+j, 1) == "*" 
								|| strExpression.Substring(-1+j, 1) == "/" )
							{
								strExpression = strExpression.Substring(-1+1, j) + "(" + strExpression.Substring(-1+j + 1);
								bReplace = true;
								i = i + 1;
								break;
							}
						}
					}
					iBrackets = 0;
					j = i;
					i = i + 1;
					while ( bReplace == true )
					{
						j = j + 1;
						if ( strExpression.Substring(-1+j, 1) == "(" )
							iBrackets = iBrackets + 1;
						if ( strExpression.Substring(-1+j, 1) == ")" )
						{
							if ( iBrackets == 0 )
							{
								strExpression = strExpression.Substring(-1+1, j - 1) + ")" + strExpression.Substring(-1+j);
								bReplace = false;
								i = i + 1;
								break;
							}
							else
								iBrackets = iBrackets - 1;
						}
			
						if ( strExpression.Substring(-1+j, 1) == "+" || strExpression.Substring(-1+j, 1) == "-" 
							|| strExpression.Substring(-1+j, 1) == "*" || strExpression.Substring(-1+j, 1) == "/" )
						{
							strExpression = strExpression.Substring(-1+1, j - 1) + ")" + strExpression.Substring(-1+j);
							bReplace = false;
							i = i + 1;
							break;
						}
					}
				}
				iLengthExpression = strExpression.Length;
				i = i + 1;
			}
			return strExpression;
		}
	

		public static double GetR(double X, double Y)
		{
			return Math.Sqrt(X * X + Y * Y);
		}

		public static double GetTheta(double X, double Y)
		{
			double dTheta;
			if ( X == 0 )
			{
				if ( Y > 0 )
					dTheta = Math.PI / 2;
				else
					dTheta = -Math.PI / 2;
			}
			else
				dTheta = DoAngleOperation(Y / X, "arctan");
    
    
			//actual range of theta is from 0 to 2PI
			if ( X < 0 )
				dTheta = dTheta + Math.PI;
			else if ( Y < 0 )
				dTheta = dTheta + 2 * Math.PI;
			return dTheta;
		}
	}
}