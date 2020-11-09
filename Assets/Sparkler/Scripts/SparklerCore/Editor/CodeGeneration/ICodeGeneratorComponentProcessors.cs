namespace Sparkler.Editor.CodeGeneration
{
	public interface ICodeGeneratorComponentProcessors
	{
		string Process( ComponentDefinition componentDefinition, string template );
	}
}