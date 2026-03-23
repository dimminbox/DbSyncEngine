using DbSyncEngine.Application.Pipelines.Common;
using Orders.Core.Models.Entites;

namespace DbSyncEngine.Application.Pipelines.Comparison;

public class AggregateDecisionService : IAggregateDecisionService
{
    public SyncOperation Decide(SyncAggregate mysql, SyncAggregate postgres)
    {
        if (postgres == null || postgres.Order == null)
            return SyncOperation.Create;

        if (AreOrdersEqual(mysql.Order, postgres.Order))
        {
            return SyncOperation.Skip;
        }

        return SyncOperation.Update;
    }

    private bool AreOrdersEqual(Order? a, Order? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        bool DateEquals(DateTime? x, DateTime? y)
            => x.HasValue == y.HasValue &&
               (!x.HasValue || x.Value == y.Value);

        bool DecimalEquals(decimal x, decimal y,  decimal epsilon = 0.001m)
            => Math.Abs(x - y) <= epsilon;

        var res =
            a.OrderGuid == b.OrderGuid &&
            a.Buffer == b.Buffer &&
            a.Active == b.Active &&
            a.GroupGuid == b.GroupGuid &&
            a.SourceGuid == b.SourceGuid &&
            DateEquals(a.DateCreate, b.DateCreate) &&
            DateEquals(a.DateUpdate, b.DateUpdate) &&
            a.ServiceCode == b.ServiceCode &&
            a.User == b.User &&
            a.Contractor == b.Contractor &&
            a.Email == b.Email &&
            a.DocumentNumber == b.DocumentNumber &&
            a.DocumentView == b.DocumentView &&
            a.Type == b.Type &&
            a.TypeValue == b.TypeValue &&
            a.Store == b.Store &&
            a.IsAutoProcessing == b.IsAutoProcessing &&
            a.NeedManualProcessing == b.NeedManualProcessing &&
            DateEquals(a.ReserveEndDate, b.ReserveEndDate) &&
            a.StatusGuid == b.StatusGuid &&
            DateEquals(a.StatusDate, b.StatusDate) &&
            a.Comment == b.Comment &&
            a.CommentUser == b.CommentUser &&
            a.Bookmark == b.Bookmark &&
            a.IsLoyalty == b.IsLoyalty &&
            a.ProductsCount == b.ProductsCount &&
            DecimalEquals(a.Total, b.Total) &&
            DecimalEquals(a.VatAmount, b.VatAmount) &&
            a.DeliveryTypeGuid == b.DeliveryTypeGuid &&
            a.DeliveryAddress == b.DeliveryAddress &&
            DateEquals(a.DeliveryDate, b.DeliveryDate) &&
            DateEquals(a.DeliveryDateDesired, b.DeliveryDateDesired) &&
            a.PaymentTypeGuid == b.PaymentTypeGuid &&
            a.ExpeditorName == b.ExpeditorName &&
            a.ExpeditorPhone == b.ExpeditorPhone &&
            a.TenderSupport == b.TenderSupport &&
            a.TypeTenderDiscount == b.TypeTenderDiscount &&
            a.Completed == b.Completed &&
            a.InBuffer1C == b.InBuffer1C &&
            a.Ispzk == b.Ispzk &&
            a.ContractorDocumentNumber == b.ContractorDocumentNumber &&
            DateEquals(a.ContractorDocumentDate, b.ContractorDocumentDate);

        return res;
    }
}