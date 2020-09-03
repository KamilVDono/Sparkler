using System.Collections.Generic;

namespace FSM
{
	public enum LaunchType
	{
		Run,
		Schedule,
		ScheduleParallel
	}

	public class FileSystemData
	{
		public string Name { get; set; }
		public FileSystemLambdaData[] Lambdas { get; set; }
	}

	public class FileSystemLambdaData
	{
		public LaunchType LaunchType { get; set; }
		public string Name { get; set; }
		public string QueryField { get; set; }
		public string SharedComponentFilter { get; set; }

		public List<string> WithAny { get; } = new List<string>();
		public List<string> WithAll { get; } = new List<string>();
		public List<string> WithNone { get; } = new List<string>();
		public List<string> InParameter { get; } = new List<string>();
		public List<string> RefParamter { get; } = new List<string>();
	}
}