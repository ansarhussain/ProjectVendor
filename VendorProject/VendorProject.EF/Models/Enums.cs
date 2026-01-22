namespace VendorProject.EF.Models
{
    public enum UserRoleType
    {
        Vendor,
        Buyer,
        Transporter,
        Admin
    }

    public enum KycStatus
    {
        Pending,
        Verified,
        Rejected
    }

    public enum ContactType
    {
        Primary,
        Support,
        Driver,
        Manager
    }

    public enum OrderStatus
    {
        Draft,
        Placed,
        Accepted,
        Rejected,
        Cancelled,
        Completed
    }

    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Refunded
    }

    public enum PriceModel
    {
        Flat,
        PerKm,
        PerKg,
        PerTon
    }

    public enum ShipmentStatus
    {
        Requested,
        Quoted,
        Confirmed,
        PickedUp,
        InTransit,
        Delivered,
        Cancelled
    }
}
