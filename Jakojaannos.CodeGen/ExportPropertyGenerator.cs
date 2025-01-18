using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Jakojaannos.CodeGen;

[Generator]
public class ExportPropertyGenerator : IIncrementalGenerator {
	public void Initialize(IncrementalGeneratorInitializationContext context) {
		context.RegisterPostInitializationOutput(ctx => {
			ctx.AddSource(
				"MustSetInEditorAttribute.generated.cs",
				SourceText.From(SourceGenerationHelper.MarkerAttribute, Encoding.UTF8)
			);
		});

		var propertiesToGenerate = context
			.SyntaxProvider
			.ForAttributeWithMetadataName(
				"Jakojaannos.CodeGen.MustSetInEditorAttribute",
				predicate: static (s, _) => true,
				transform: static (ctx, _) => GetPropertyToGenerate(ctx.SemanticModel, ctx.TargetNode)
			)
			.Where(static m => m is not null);

		context.RegisterSourceOutput(
			propertiesToGenerate,
			static (spc, source) => Execute(source, spc)
		);
	}

	private static void Execute(PropertyToGenerate? propertyToGenerate, SourceProductionContext context) {
		if (propertyToGenerate is PropertyToGenerate value) {
			var result = SourceGenerationHelper.GeneratePropertyExtensionClass(value);
			context.AddSource(
				$"PropertyExtensions.{value.Namespace}.{value.ClassName}.{value.Name}.generated.cs",
				SourceText.From(result, Encoding.UTF8)
			);
		}
	}

	private static PropertyToGenerate? GetPropertyToGenerate(SemanticModel semanticModel, SyntaxNode syntaxNode) {
		if (semanticModel.GetDeclaredSymbol(syntaxNode) is not IPropertySymbol propertySymbol) {
			// Could not determine the field symbol
			return null;
		}


		var containingClass = propertySymbol.ContainingType;
		if (containingClass.Name is not string className) {
			// The field is not contained within a type, which is... odd? Skip it.
			return null;
		}
		var namespaceName = propertySymbol.ContainingNamespace is null || propertySymbol.ContainingNamespace.IsGlobalNamespace
			? null
			: propertySymbol.ContainingNamespace?.ToString();

		var fieldName = propertySymbol.Name;

		// HACK: this is a really messy way of doing this
		var typeName = propertySymbol.Type.ToString().TrimEnd('?');
		return new PropertyToGenerate(fieldName, typeName, className, namespaceName);
	}
}
