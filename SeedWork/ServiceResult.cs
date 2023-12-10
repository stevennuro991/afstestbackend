namespace Afstest.API.SeedWork
{
    public class ServiceResult
    {
        public object Entity { get; set; } = default!;

        public List<string> Errors { get; set; } = new List<string>();

        public bool HasErrors => Errors.Any();

        public ServiceResult()
        {

        }

        public ServiceResult(string errorMessage)
        {
            Errors.Add(errorMessage);
        }

        public ServiceResult(IList<string> errors)
        {
            Errors.AddRange(errors);
        }

        public ServiceResult(object entity)
        {
            Entity = entity;
        }
    }
}
