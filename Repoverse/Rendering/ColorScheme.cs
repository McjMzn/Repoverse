namespace Repoverse.Rendering
{
    internal class ColorScheme
    {
        public string RepositoryNode { get; set; }
        public string NonRepositoryNode { get; set; }
        public string RootNode { get; set; }
        public string ActiveRepository { get; set; }
        public string InactiveRepository { get; set; }
        public string RecentSuccessResult { get; set; }
        public string RecentErrorResult { get; set; }
        public string ActiveProcess { get; set; }
        public string ErrorResult { get; set; }
        public string SuccessResult { get; set; }
        public string RepositoryBranch { get; set; }

        public static ColorScheme Default { get; } = new ColorScheme
        {
            RepositoryNode = "blue",
            NonRepositoryNode = "default",
            RootNode = "yellow",
            ActiveRepository = "white",
            InactiveRepository = "grey",
            ActiveProcess = "yellow",
            RecentSuccessResult = "lime",
            SuccessResult = "green4",
            RecentErrorResult = "red",
            ErrorResult = "red3_1",
            RepositoryBranch = "cyan"
        };
    }
}
