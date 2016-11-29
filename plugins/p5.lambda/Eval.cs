/*
 * Phosphorus Five, copyright 2014 - 2016, Thomas Hansen, mr.gaia@gaiasoul.com
 * 
 * This file is part of Phosphorus Five.
 *
 * Phosphorus Five is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 3, as published by
 * the Free Software Foundation.
 *
 *
 * Phosphorus Five is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Phosphorus Five.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * If you cannot for some reasons use the GPL license, Phosphorus
 * Five is also commercially available under Quid Pro Quo terms. Check 
 * out our website at http://gaiasoul.com for more details.
 */

using System.Linq;
using System.Collections.Generic;
using p5.exp;
using p5.core;
using p5.exp.exceptions;

/// <summary>
///     Main namespace for p5 lambda keywords
/// </summary>
namespace p5.lambda
{
    /// <summary>
    ///     Class wrapping all [eval] keywords in p5 lambda
    /// </summary>
    public static class Eval
    {
        // Used to extract commonalities for eval Active Events
        private delegate void ExecuteFunctor (
            ApplicationContext context, 
            Node exe, 
            Node evalNode, 
            IEnumerable<Node> args,
            bool isFirst);

        /// <summary>
        ///     Executes a specified piece of p5 lambda block as a copied lambda object
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "eval")]
        public static void eval (ApplicationContext context, ActiveEventArgs e)
        {
            Executor (ExecuteBlockCopy, context, e.Args, e.Args.Name != "eval");
        }
        
        /// <summary>
        ///     Executes a specified piece of p5 lambda block as mutable
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "eval-mutable")]
        public static void eval_mutable (ApplicationContext context, ActiveEventArgs e)
        {
            Executor (ExecuteBlockMutable, context, e.Args, e.Args.Name != "eval-mutable");
        }

        /*
         * Worker method for [eval]
         */
        private static void Executor (
            ExecuteFunctor functor, 
            ApplicationContext context, 
            Node args, 
            bool forceChildren)
        {
            // Checking if we should foce execution of children nodes, and not evaluate expressions in main node
            if (forceChildren || args.Value == null) {

                // Evaluating current scope
                functor (context, args, args, new Node[] {}, true);
            } else {

                // Evaluating a value object or an expression, making sure we let functor know which
                // was the first invocation
                bool isFirst = true;
                foreach (var idxSource in XUtil.Iterate<Node> (context, args)) {

                    // Evaluating currently iterated source
                    functor (context, idxSource, args, args.Children, isFirst);
                    isFirst = false;
                }
            }
        }

        /*
         * Executes a block of nodes by copying the nodes executed, and executing the copy, 
         * returning anything created inside of the block back to caller
         */
        private static void ExecuteBlockCopy (
            ApplicationContext context, 
            Node exe, 
            Node evalNode, 
            IEnumerable<Node> args,
            bool isFirst)
        {
            // Making sure lambda is executed on copy of execution nodes,
            // without access to nodes outside of its own scope
            Node exeCopy = exe.Clone ();

            // Passing in arguments, if there are any
            foreach (var idx in args.Reverse ()) {
                exeCopy.Insert (0, idx.Clone ());
            }

            // Storing the original nodes before execution, such that we can "diff" against nodes after execution,
            // to make it possible to return ONLY added nodes after execution
            List<Node> originalNodes = new List<Node> (exeCopy.Children);

            // Actual execution of nodes
            ExecuteAll (exeCopy, context);

            // Checking if we returned prematurely due to [return] invocation
            if (exeCopy.FirstChild != null && exeCopy.FirstChild.Name == "_return")
                exeCopy.FirstChild.UnTie ();

            // Returning all nodes created inside of execution block, and ONLY these nodes, plus value of lambda block.
            // Notice, this means clearing the evalNode's children collection ONLY the first time we execute it
            if (isFirst)
                evalNode.Clear ();
            evalNode.AddRange (exeCopy.Children.Where (ix => originalNodes.IndexOf (ix) == -1));
            evalNode.Value = exeCopy.Value;
        }
        
        /*
         * Executes a block of nodes in mutable state
         */
        private static void ExecuteBlockMutable (
            ApplicationContext context, 
            Node exe, 
            Node evalNode, 
            IEnumerable<Node> args,
            bool isFirst)
        {
            // Passing in arguments, if there are any
            foreach (var idx in args) {
                exe.Add (idx.Clone ());
            }

            // Actual execution of block
            ExecuteAll (exe, context);

            // Checking if we returned prematurely due to [return] invocation
            if (exe.FirstChild != null && exe.FirstChild.Name == "_return")
                exe.FirstChild.UnTie ();
        }

        /*
         * Executes one execution statement
         */
        private static void ExecuteAll (Node exe, ApplicationContext context)
        {
            // Iterating through all nodes in execution scope, unless [offset] is given, and raising these as Active Events
            Node idxExe = null;

            // Checking if we have an [offset]
            if (exe ["offset"] != null) {

                // Retrieving offset
                int offset = exe ["offset"].Get<int> (context);

                // Checking if execution block is "empty"
                if (offset == exe.Children.Count)
                    return;

                // Checking offset is not larger than number of children in current lambda
                if (offset > exe.Children.Count)
                    throw new LambdaException ("[offset] was too large for lambda block, couldn't find that many children", exe, context);

                // Setting first execution statement as the offset node
                idxExe = exe [offset];
            } else {

                // No [offset] given, executing everything
                idxExe = exe.FirstChild;
            }

            // Looping as long as we've got more nodes in scope
            while (idxExe != null) {

                // Storing "next execution node" as fallback, to support "delete this node" logic
                var nextFallback = idxExe.NextSibling;

                // We don't execute nodes that start with an underscore "_" since these are considered "data segments", in addition
                // to nodes starting with ".", since these are considered lambda callbacks.
                // In addition, we don't execute nodes with no name, since these interfers with "null Active Event handlers"
                if (!idxExe.Name.StartsWith ("_") && !idxExe.Name.StartsWith (".") && idxExe.Name != "") {

                    // Raising the given Active Event.
                    context.Raise (idxExe.Name, idxExe);
                }

                // Checking if we're supposed to return from evaluation
                var rootChildName = exe.Root.FirstChild != null ? exe.Root.FirstChild.Name : null;
                if (rootChildName == "_return" || rootChildName == "_break" || rootChildName == "_continue")
                    return; // Breaking evaluation of any further code

                // Prioritizing "NextSibling", in case this node created new nodes, while having
                // nextFallback as "fallback node", in case current execution node removed current execution node.
                // But in case nextFallback also was removed, we set idxExe to null, breaking the while loop
                idxExe = idxExe.NextSibling ?? (nextFallback != null && nextFallback.Parent != null ? nextFallback : null);
            }
        }
    }
}
