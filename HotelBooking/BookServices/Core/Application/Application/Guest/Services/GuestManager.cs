using Application.Guest.DTO;
using Application.Guest.Port;
using Application.Guest.Request;
using Application.Guest.Responses;
using Domain.Exceptions;
using Domain.Ports;

namespace Application.Guest.Services;

public class GuestManager : IGuestManager
{
    private IGuestRepository _guestRepository;
    public GuestManager(IGuestRepository guestRepository) => _guestRepository = guestRepository;

    public async Task<GuestResponse> CreateGuest(CreateGuestRequest request)
    {
        try
        {
            Domain.Entities.Guest guest = GuestDTO.MapToEntity(request.Data);

            await guest.Save(_guestRepository);

            request.Data.Id = guest.Id;

            return new GuestResponse
            {
                Data = request.Data,
                Success = true
            };
        }
        catch (EmailAlreadyUseException)
        {
            return new GuestResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.INVALID_EMAIL,
                Message = "The provided email is already in use."
            };
        }
        catch (InvalidPersonDocumentIdException)
        {
            return new GuestResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.INVALID_PERSON_DOCUMENT_ID,
                Message = "The provided document id is not valid."
            };
        }
        catch (MissingRequiredInformationException)
        {
            return new GuestResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.MISSING_REQUIRED_INFORMATION,
                Message = "Missing required information."
            };
        }
        catch (InvalidEmailException)
        {
            return new GuestResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.INVALID_EMAIL,
                Message = "The provided email is not valid."
            };
        }
        catch (Exception)
        {
            return new GuestResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.UNEXPECTED_ERROR,
                Message = "Not able to store the guest data. Please try again later."
            };
        }
    }

    public async Task<GuestResponse> GetById(int id)
    {
        GuestResponse response = new GuestResponse();

        try
        {
            Domain.Entities.Guest? guest = await _guestRepository.FindById(id);

            if (guest is null)
            {
                response.Success = false;
                response.ErrorCode = ErrorCodes.NOT_FOUND;
                response.Message = "Guest not found.";
            }
            else
            {
                response.Success = true;
                response.Data = GuestDTO.MapFromEntity(guest);
            }

            return response;
        }
        catch (Exception)
        {
            response.Success = false;
            response.ErrorCode = ErrorCodes.UNEXPECTED_ERROR;
            response.Message = "Not able to retrieve the guest data. Please try again later.";
            return response;
        }
    }
}