using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace JetBrains.ReSharper.Plugins.QuirkyFormattings.Psi.CodeStyle.Formatting
{
  public static class QuirkyCSharpUtils
  {
    public static bool HasLocalFunctionInvocation(this IExpressionStatement expressionStatement)
    {
      return expressionStatement.GetInvokedLocalFunctionDeclarations().Any();
    }

    private static IEnumerable<IBlock> EnumerateContainingScopes(this ITreeNode node)
    {
      for (var trav = node; trav != null; trav = trav.Parent)
        if (trav is IBlock block) yield return block;
    }

    public static IEnumerable<ILocalFunctionDeclaration> GetInvokedLocalFunctionDeclarations(
      this IExpressionStatement expressionStatement
    )
    {
      if (expressionStatement.Expression is not IInvocationExpression invokedExpr) 
        return Enumerable.Empty<ILocalFunctionDeclaration>();

      var declarations = new List<ILocalFunctionDeclaration>();

      foreach (var containingScope in expressionStatement.EnumerateContainingScopes())
      {
        declarations.AddRange(
          containingScope.Children<IDeclarationStatement>()
            .Where(
              decl => decl.LocalFunctionDeclaration != null 
                      && decl.LocalFunctionDeclaration.NameIdentifier.Name == invokedExpr.Reference.GetName()
            )
            .Select(decl => decl.LocalFunctionDeclaration)
        );
      }

      return declarations;
    }
  }
}