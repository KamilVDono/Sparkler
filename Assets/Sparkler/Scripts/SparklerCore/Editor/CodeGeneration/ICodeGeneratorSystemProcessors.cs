using Sparkler.Components;

namespace Sparkler.Editor.CodeGeneration
{
	public interface ICodeGeneratorSystemProcessorBeforeLambda
	{
		string ProcessBeforeLambda( SystemsGraph systemsGraph, SystemNode system, string template );
	}

	public interface ICodeGeneratorSystemProcessorAfterLambda
	{
		string ProcessAfterLambda( SystemsGraph systemsGraph, SystemNode system, string template );
	}

	public interface ICodeGeneratorSystemProcessorLambda
	{
		string ProcessLambda( SystemNode system, SystemLambdaAction lambda, string currentTemplate );
	}
}