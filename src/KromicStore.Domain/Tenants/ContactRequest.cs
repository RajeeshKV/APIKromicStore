using KromicStore.Domain.Common;

namespace KromicStore.Domain.Tenants;

/// <summary>
/// Represents a contact request from the landing page or public form.
/// Tracks inquiries from potential tenants or customers.
/// </summary>
public sealed class ContactRequest : AuditableEntity
{
    private readonly List<ContactRequestReply> _replies = [];

    private ContactRequest()
    {
        Name = string.Empty;
        Email = string.Empty;
        Subject = string.Empty;
        Message = string.Empty;
    }

    private ContactRequest(Guid id, string name, string email, string subject, string message)
        : base(id)
    {
        Name = name;
        Email = email;
        Subject = subject;
        Message = message;
        Status = ContactRequestStatus.New;
        ReceivedOnUtc = DateTime.UtcNow;
    }

    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Subject { get; private set; }
    public string Message { get; private set; }
    public ContactRequestStatus Status { get; private set; }
    public string? Category { get; private set; } // Sales, Support, Partnership, etc.
    public DateTime ReceivedOnUtc { get; private set; }
    public DateTime? ResolvedOnUtc { get; private set; }
    public Guid? ResolvedByUserId { get; private set; }
    public string? InternalNotes { get; private set; }
    public IReadOnlyList<ContactRequestReply> Replies => _replies.AsReadOnly();

    public static ContactRequest Create(
        string name,
        string email,
        string subject,
        string message,
        string? phoneNumber = null,
        string? category = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required.", nameof(subject));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message is required.", nameof(message));

        var request = new ContactRequest(Guid.NewGuid(), name.Trim(), email.Trim(), subject.Trim(), message.Trim())
        {
            PhoneNumber = phoneNumber ?? string.Empty,
            Category = category
        };

        return request;
    }

    public void MarkAsRead()
    {
        if (Status == ContactRequestStatus.New)
            Status = ContactRequestStatus.Read;
    }

    public void Assign(Guid userId)
    {
        Status = ContactRequestStatus.Assigned;
    }

    public void Reply(string message, Guid repliedByUserId)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Reply message is required.", nameof(message));

        var reply = ContactRequestReply.Create(Id, message, repliedByUserId);
        _replies.Add(reply);

        if (Status == ContactRequestStatus.New || Status == ContactRequestStatus.Read)
            Status = ContactRequestStatus.InProgress;
    }

    public void Resolve(Guid resolvedByUserId, string? notes = null)
    {
        Status = ContactRequestStatus.Resolved;
        ResolvedOnUtc = DateTime.UtcNow;
        ResolvedByUserId = resolvedByUserId;
        InternalNotes = notes;
    }

    public void Archive()
    {
        Status = ContactRequestStatus.Archived;
    }

    public void Reopen()
    {
        if (Status != ContactRequestStatus.Resolved && Status != ContactRequestStatus.Archived)
            return;

        Status = ContactRequestStatus.InProgress;
        ResolvedOnUtc = null;
        ResolvedByUserId = null;
    }
}

public enum ContactRequestStatus
{
    New = 0,
    Read = 1,
    Assigned = 2,
    InProgress = 3,
    Resolved = 4,
    Archived = 5
}

/// <summary>
/// Reply to a contact request from Super User.
/// </summary>
public sealed class ContactRequestReply : AuditableEntity
{
    private ContactRequestReply()
    {
        Message = string.Empty;
    }

    private ContactRequestReply(Guid id, Guid contactRequestId, string message, Guid repliedByUserId)
        : base(id)
    {
        ContactRequestId = contactRequestId;
        Message = message;
        RepliedByUserId = repliedByUserId;
    }

    public Guid ContactRequestId { get; private set; }
    public string Message { get; private set; }
    public Guid RepliedByUserId { get; private set; }
    public bool WasEmailSentToRequester { get; private set; }

    public static ContactRequestReply Create(
        Guid contactRequestId,
        string message,
        Guid repliedByUserId)
    {
        if (contactRequestId == Guid.Empty)
            throw new ArgumentException("ContactRequestId is required.", nameof(contactRequestId));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message is required.", nameof(message));
        if (repliedByUserId == Guid.Empty)
            throw new ArgumentException("RepliedByUserId is required.", nameof(repliedByUserId));

        return new ContactRequestReply(Guid.NewGuid(), contactRequestId, message.Trim(), repliedByUserId);
    }

    public void MarkEmailSent() => WasEmailSentToRequester = true;
}
