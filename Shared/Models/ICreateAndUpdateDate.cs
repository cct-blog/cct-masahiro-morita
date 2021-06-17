using System;

namespace blazorTest.Shared.Models
{
    public interface ICreateAndUpdateDate
    {
        DateTime CreateDate { get; set; }

        DateTime UpdateDate { get; set; }

        public void UpdateNow() => CreateDate = UpdateDate = DateTime.Now;
    }

    public interface IReadonlyCreateAndUpdateDate
    {
        DateTime CreateDate { get; init; }

        DateTime UpdateDate { get; init; }
    }
}