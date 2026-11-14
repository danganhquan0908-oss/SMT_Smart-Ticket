using System.Text.Json;
using BusGoSmartTicket.Exceptions.Bookings;
using BusGoSmartTicket.Exceptions.Catalog;
using BusGoSmartTicket.Exceptions.Operations;
using BusGoSmartTicket.Exceptions.Operators;
using BusGoSmartTicket.Exceptions.Payments;
using Microsoft.EntityFrameworkCore;

namespace BusGoSmartTicket.Middleware;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (CatalogNotFoundException ex) when (context.Request.Path.StartsWithSegments("/api"))
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (CatalogConflictException ex) when (context.Request.Path.StartsWithSegments("/api"))
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (OperatorNotFoundException ex) when (context.Request.Path.StartsWithSegments("/api"))
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (OperatorConflictException ex) when (context.Request.Path.StartsWithSegments("/api"))
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (BookingNotFoundException ex) when (context.Request.Path.StartsWithSegments("/api"))
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (BookingConflictException ex) when (context.Request.Path.StartsWithSegments("/api"))
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (PaymentNotFoundException ex) when (context.Request.Path.StartsWithSegments("/api"))
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (PaymentConflictException ex) when (context.Request.Path.StartsWithSegments("/api"))
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (OperationsNotFoundException ex) when (context.Request.Path.StartsWithSegments("/api"))
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (OperationsConflictException ex) when (context.Request.Path.StartsWithSegments("/api"))
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (DbUpdateConcurrencyException) when (context.Request.Path.StartsWithSegments("/api"))
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Dữ liệu chuyến vừa được cập nhật bởi một yêu cầu khác. Tải lại và thử lại.");
        }
        catch (Exception ex) when (context.Request.Path.StartsWithSegments("/api"))
        {
            logger.LogError(ex, "Lỗi khi xử lý API danh mục.");
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Đã xảy ra lỗi máy chủ.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int status, string detail)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status,
            title = status == 409 ? "Xung đột dữ liệu" : status == 404 ? "Không tìm thấy" : "Lỗi máy chủ",
            detail
        }));
    }
}
