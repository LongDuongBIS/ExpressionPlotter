namespace ExpressionPlotterControl
{
    public interface IEvaluatable
    {
        /// <summary>
        /// Should return text for the expression
        /// </summary>
        string ExpressionText
        {
            get;
            set;
        }

        /// <summary>
        /// Should return true if the expression is valid and can be evaluated
        /// </summary>
        bool IsValid
        {
            get;
        }

        ///<summary>This method should evaluate the expression and return the result as double. It should return double.NaN in the case the expression cant be evaluated e.g. log( -ve no. )</summary>
        ///<param name="dvalueX">The value of X at which we want to evaluate the expression</param>
        ///<returns>The result of expression evaluation as a double</returns>
        double Evaluate(double dvalueX);
    }
}