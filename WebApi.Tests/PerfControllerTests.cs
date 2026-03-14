using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;

public class PerfControllerTests
{
    [Fact]
    public void GetItems_DefaultCount_Returns100Items()
    {
        var controller = new PerfController();
        var result = controller.GetItems();

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsType<PerfItemDto[]>(ok.Value);
        Assert.Equal(100, items.Length);
    }

    [Fact]
    public void GetItems_Count0_ReturnsEmpty()
    {
        var controller = new PerfController();
        var result = controller.GetItems(0);

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsType<PerfItemDto[]>(ok.Value);
        Assert.Empty(items);
    }

    [Fact]
    public void GetItems_Count1_ReturnsSingleItem()
    {
        var controller = new PerfController();
        var result = controller.GetItems(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsType<PerfItemDto[]>(ok.Value);
        Assert.Single(items);
        Assert.Equal(0, items[0].Id);
        Assert.Equal("Item 0", items[0].Name);
    }

    [Fact]
    public void GetItems_Count100_Returns100()
    {
        var controller = new PerfController();
        var result = controller.GetItems(100);

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsType<PerfItemDto[]>(ok.Value);
        Assert.Equal(100, items.Length);
    }

    [Fact]
    public void GetItems_Count1000_Returns1000()
    {
        var controller = new PerfController();
        var result = controller.GetItems(1_000);

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsType<PerfItemDto[]>(ok.Value);
        Assert.Equal(1_000, items.Length);
    }

    [Fact]
    public void GetItems_Count10000_Returns10000()
    {
        var controller = new PerfController();
        var result = controller.GetItems(10_000);

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsType<PerfItemDto[]>(ok.Value);
        Assert.Equal(10_000, items.Length);
    }

    [Fact]
    public void GetItems_NegativeCount_ReturnsBadRequest()
    {
        var controller = new PerfController();
        var result = controller.GetItems(-1);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);
    }

    [Fact]
    public void GetItems_CountAboveMax_ReturnsBadRequest()
    {
        var controller = new PerfController();
        var result = controller.GetItems(10_001);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);
    }

    [Fact]
    public void GetItems_ReturnsSequentialIds()
    {
        var controller = new PerfController();
        var result = controller.GetItems(10);

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsType<PerfItemDto[]>(ok.Value);

        for (var i = 0; i < items.Length; i++)
        {
            Assert.Equal(i, items[i].Id);
        }
    }

    [Fact]
    public void GetItems_ReturnsExpectedLastItemName()
    {
        var controller = new PerfController();
        var result = controller.GetItems(10);

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsType<PerfItemDto[]>(ok.Value);
        Assert.Equal("Item 9", items[^1].Name);
    }
}

