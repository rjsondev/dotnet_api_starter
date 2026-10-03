namespace DotnetApiStarter.Domain.Common;

public class BaseEntity
{
    public DateTimeOffset CreatedDate { get; set; }

    public int CreatedById { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }

    public int? ModifiedById { get; set; }

    public DateTimeOffset? DeletedDate { get; set; }

    public int? DeletedById { get; set; }
}
