using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Thry.ThryEditor
{
    public static class ExpressionParser
    {
        public static Delegate Parse(string expression)
        {
            // Clean up the expression
            expression = expression.Replace(" ", "");

            if(double.TryParse(expression, out _))
            {
                string oldExpression = expression;
                expression = $"x == {expression}";
                Debug.Log($"Expression was {oldExpression} so we assume {expression}");
            }

            // Handle negative numbers by replacing unary minus with a special character
            expression = HandleUnaryOperators(expression);

            // Define the parameter for the expression
            var parameter = Expression.Parameter(typeof(double), "x");

            // Tokenize the expression
            var tokens = Tokenize(expression);

            // Check if the parameter "x" is used in the expression
            bool containsParameter = tokens.Contains("x");

            // Parse the expression string into an expression tree
            var body = ParseExpression(tokens, containsParameter ? parameter : null);

            // Create a lambda expression with the parameter and parsed body
            if(containsParameter)
            {
                return Expression.Lambda<Func<double, bool>>(body, parameter).Compile();
            }
            else
            {
                // If parameter "x" is not used, create a parameter-less lambda
                return Expression.Lambda<Func<bool>>(body).Compile();
            }
        }

        private static string HandleUnaryOperators(string expression)
        {
            // Replace unary minus and plus with special characters
            expression = Regex.Replace(expression, @"(?<![\d)])-", "_"); // Replace unary minus
            expression = Regex.Replace(expression, @"(?<![\d)])\+", ""); // Remove unary plus
            return expression;
        }

        private static List<string> Tokenize(string expression)
        {
            var tokens = new List<string>();
            var token = "";

            for(int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];
                if(char.IsWhiteSpace(c))
                {
                    continue;
                }

                if(IsParenthesis(c))
                {
                    if(token.Length > 0)
                    {
                        tokens.Add(token);
                        token = "";
                    }

                    tokens.Add(c.ToString());
                }
                else if(i + 1 < expression.Length && IsOperator(expression.Substring(i, 2)))
                {
                    if(token.Length > 0)
                    {
                        tokens.Add(token);
                        token = "";
                    }

                    tokens.Add(expression.Substring(i, 2));
                    i++; // Skip the next character as it has been processed
                }
                else if(IsOperator(c.ToString()))
                {
                    if(token.Length > 0)
                    {
                        tokens.Add(token);
                        token = "";
                    }

                    tokens.Add(c.ToString());
                }
                else if(char.IsDigit(c) || c == '.')
                {
                    token += c;
                }
                else if(c == 'x')
                {
                    if(token.Length > 0)
                    {
                        tokens.Add(token);
                        token = "";
                    }

                    tokens.Add("x");
                }
                else
                {
                    token += c;
                }
            }

            if(token.Length > 0)
            {
                tokens.Add(token);
            }

            return tokens;
        }

        private static Expression ParseExpression(List<string> tokens, ParameterExpression parameter)
        {
            var stack = new Stack<Expression>();
            var operatorStack = new Stack<string>();

            for(int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                if(double.TryParse(token, out var number))
                {
                    stack.Push(Expression.Constant(number));
                }
                else if(token == "x")
                {
                    if(parameter == null)
                        throw new ArgumentException("Parameter 'x' is not defined.");
                    stack.Push(parameter);
                }
                else if(token == "_")
                {
                    stack.Push(Expression.Negate(ParseExpression(tokens.GetRange(i + 1, 1), parameter)));
                    i++; // Skip the next token as it has been processed
                }
                else if(IsOperator(token))
                {
                    while(operatorStack.Count > 0 && GetPrecedence(operatorStack.Peek()) >= GetPrecedence(token))
                    {
                        var op = operatorStack.Pop();
                        var right = stack.Pop();
                        var left = stack.Pop();
                        stack.Push(CreateBinaryExpression(op, left, right));
                    }

                    operatorStack.Push(token);
                }
                else if(token == "(")
                {
                    operatorStack.Push(token);
                }
                else if(token == ")")
                {
                    while(operatorStack.Count > 0 && operatorStack.Peek() != "(")
                    {
                        var op = operatorStack.Pop();
                        var right = stack.Pop();
                        var left = stack.Pop();
                        stack.Push(CreateBinaryExpression(op, left, right));
                    }

                    if(operatorStack.Count > 0 && operatorStack.Peek() == "(")
                    {
                        operatorStack.Pop();
                    }
                }
                else
                {
                    throw new ArgumentException($"Invalid token: {token}");
                }
            }

            while(operatorStack.Count > 0)
            {
                var op = operatorStack.Pop();
                var right = stack.Pop();
                var left = stack.Pop();
                stack.Push(CreateBinaryExpression(op, left, right));
            }

            return stack.Pop();
        }

        private static Expression CreateBinaryExpression(string op, Expression left, Expression right)
        {
            switch(op)
            {
                case "+":
                    return Expression.Add(left, right);
                case "-":
                    return Expression.Subtract(left, right);
                case "*":
                    return Expression.Multiply(left, right);
                case "/":
                    return Expression.Divide(left, right);
                case "%":
                    return Expression.Modulo(left, right);
                case "^":
                    return Expression.Power(left, right);
                case "==":
                    return Expression.Equal(left, right);
                case "!=":
                    return Expression.NotEqual(left, right);
                case "<":
                    return Expression.LessThan(left, right);
                case "<=":
                    return Expression.LessThanOrEqual(left, right);
                case ">":
                    return Expression.GreaterThan(left, right);
                case ">=":
                    return Expression.GreaterThanOrEqual(left, right);
                case "&&":
                    return Expression.AndAlso(left, right);
                case "||":
                    return Expression.OrElse(left, right);
                default:
                    throw new ArgumentException($"Invalid operator: {op}");
            }
        }


        static readonly string[] _operators = new string[] { "+", "-", "*", "/", "%", "^", "==", "!=", "<", "<=", ">", ">=", "&&", "||", "_" };

        private static bool IsOperator(string token)
        {
            return _operators.Contains(token);
        }

        private static bool IsParenthesis(char c)
        {
            return c == '(' || c == ')';
        }

        private static int GetPrecedence(string op)
        {
            switch(op)
            {
                case "||":
                    return 1;
                case "&&":
                    return 2;
                case "==":
                case "!=":
                    return 3;
                case "<":
                case "<=":
                case ">":
                case ">=":
                    return 4;
                case "+":
                case "-":
                    return 5;
                case "*":
                case "/":
                case "%":
                    return 6;
                case "^":
                    return 7;
                default:
                    return 0;
            }
        }

        private class ExpressionReplacer : ExpressionVisitor
        {
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return Expression.Parameter(typeof(double), "x");
            }
        }
    }
}