using AutoMapper;
using ContactManagement.Core.DTOs.Common;
using ContactManagement.Core.DTOs.Contacts;
using ContactManagement.Core.Entities;
using ContactManagement.Core.Exceptions;
using ContactManagement.Core.Interfaces;
using ContactManagement.Core.Interfaces.Repositories;
using ContactManagement.Services.Mappings;
using ContactManagement.Services.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ContactManagement.Tests.Services;

public sealed class ContactServiceTests
{
    private static ContactService CreateService(
        Mock<IUnitOfWork> unitOfWork,
        Mock<IContactRepository> contacts,
        Mock<ICategoryRepository> categories)
    {
        unitOfWork.SetupGet(u => u.Contacts).Returns(contacts.Object);
        unitOfWork.SetupGet(u => u.Categories).Returns(categories.Object);

        var mapper = new MapperConfiguration(
                config => config.AddProfile<MappingProfile>(),
                NullLoggerFactory.Instance)
            .CreateMapper();

        return new ContactService(unitOfWork.Object, mapper, NullLogger<ContactService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsCreatedContact()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var contacts = new Mock<IContactRepository>();
        var categories = new Mock<ICategoryRepository>();

        contacts.Setup(r => r.EmailExistsForUserAsync(1, "john@example.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        contacts.Setup(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .Callback<Contact, CancellationToken>((c, _) => c.ContactId = 10)
            .Returns(Task.CompletedTask);
        contacts.Setup(r => r.GetByIdForUserAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact
            {
                ContactId = 10,
                UserId = 1,
                FullName = "John Doe",
                Email = "john@example.com",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

        var service = CreateService(unitOfWork, contacts, categories);

        var result = await service.CreateAsync(1, new CreateContactRequest
        {
            FullName = "John Doe",
            Email = "john@example.com"
        });

        Assert.True(result.Success);
        Assert.Equal(10, result.Data!.ContactId);
        Assert.Equal("John Doe", result.Data.FullName);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ThrowsConflict()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var contacts = new Mock<IContactRepository>();
        var categories = new Mock<ICategoryRepository>();

        contacts.Setup(r => r.EmailExistsForUserAsync(1, "john@example.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService(unitOfWork, contacts, categories);

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(1, new CreateContactRequest
        {
            FullName = "John Doe",
            Email = "john@example.com"
        }));
    }

    [Fact]
    public async Task GetByIdAsync_WhenContactBelongsToAnotherUser_ThrowsNotFound()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var contacts = new Mock<IContactRepository>();
        var categories = new Mock<ICategoryRepository>();

        contacts.Setup(r => r.GetByIdForUserAsync(2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var service = CreateService(unitOfWork, contacts, categories);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(2, 10));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedContacts()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var contacts = new Mock<IContactRepository>();
        var categories = new Mock<ICategoryRepository>();

        IReadOnlyList<Contact> items = new List<Contact>
        {
            new() { ContactId = 1, UserId = 1, FullName = "Jane", Email = "jane@example.com" }
        };

        contacts.Setup(r => r.GetPagedAsync(1, It.IsAny<ContactQueryParams>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, 1));

        var service = CreateService(unitOfWork, contacts, categories);

        var result = await service.GetAllAsync(1, new ContactQueryParams { Page = 1, PageSize = 10 });

        Assert.True(result.Success);
        Assert.Equal(1, result.Data!.TotalCount);
        Assert.Single(result.Data.Items);
    }

    [Fact]
    public async Task SetFavoriteAsync_WithInvalidId_ThrowsValidationException()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var contacts = new Mock<IContactRepository>();
        var categories = new Mock<ICategoryRepository>();
        var service = CreateService(unitOfWork, contacts, categories);

        await Assert.ThrowsAsync<ValidationException>(() => service.SetFavoriteAsync(1, 0, true));
    }
}
