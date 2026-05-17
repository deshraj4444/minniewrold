using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using MayaAstro.Models;
using MayaAstro.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MayaAstro.Controllers.WebApi
{
    [ApiController]
    [Authorize]
    public class LectureAPIController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;
        private const string FcmUrl = "https://fcm.googleapis.com/fcm/send";
        private const string ServerKey = "BOB9zEd8l2tzJNG9gfdyJDeGXxD5Ig8vBP-ibnRilnbtzgf8NjD3WuJL6b9gGCiyFz1IJb7RD4J5qg9FDijfSh4";
        private readonly IAdminServices _iAdminServices;

        public LectureAPIController(IAdminServices iAdminServices)
        {
            _iAdminServices = iAdminServices;
            if (FirebaseApp.DefaultInstance == null)
            {
                // Initialize Firebase app only if it hasn't been initialized yet
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("gserviceaccount.json")  // Set path to your downloaded service account key
                });
            }
        }



        [Route("api/notifications")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationModel model)
        {
            var message = new Message()
            {
                Token = model.Token,
                Notification = new Notification()
                {
                    Title = model.Title,
                    Body = model.Body
                }
            };

            try
            {
                // Send the message using Firebase Admin SDK
                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

                // Return success response
                return Ok();
            }
            catch (FirebaseAdmin.Messaging.FirebaseMessagingException ex)
            {
                // Handle invalid token error
                if (ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
                {
                    // Log invalid token for further analysis
                    Console.WriteLine($"Invalid token detected: {model.Token}");
                    return BadRequest($"Invalid token detected: {ex.Message}");
                }
                else
                {
                    // Handle other FCM errors
                    Console.WriteLine($"FCM error: {ex.MessagingErrorCode} - {ex.Message}");
                    return StatusCode(500, $"FCM error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                // Handle general errors
                Console.WriteLine($"Error sending notification: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        [Route("api/SaveLecture/")]
        public async Task<IActionResult> AddLecture(LectureCategoryVM obj)
        {
            try
            {
                string lectureName = obj.LectureName;
                // Add Lecture
                var data = await _iAdminServices.AddLecture(obj);

                if (data != null) // Assuming a successful addition returns a non-null response
                {
                    // Fetch all device tokens
                    var tokens = await _iAdminServices.GetDeviceToken();

                    if (tokens == null || !tokens.Any())
                    {
                        return Ok(new { message = "Lecture added, but no device tokens found for notification." });
                    }

                    // Use a HashSet to track unique tokens
                    var uniqueTokens = new HashSet<string>(tokens);

                    foreach (var token in uniqueTokens)
                    {
                        try
                        {
                            var notification = new NotificationModel
                            {
                                Token = token,
                                Title = "New exercises are now available in 'German Grammar'.",
                                Body = $"Lecture {lectureName} available."
                            };

                            await SendNotification(notification);
                        }
                        catch (Exception ex)
                        {
                            // Log error for this token but continue with the next
                            Console.WriteLine($"Error sending notification to token {token}: {ex.Message}");
                        }
                    }

                    return Ok(new { message = "Lecture added and notifications sent successfully!" });
                }

                return BadRequest("Failed to add Lecture.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        #region Lecture for App



        #region Lecture Category

        [HttpGet]
        [Route("api/GetLectureList/")]
        public async Task<IActionResult> GetLectureList()

        {
            var data = await _iAdminServices.GetLectureList();
            return Ok(data);
        }
        [HttpGet]
        [Route("api/GetLectureListByCategory/{categoryId}")]
        public async Task<IActionResult> GetLectureListByCategory(int categoryId)
        {
            var data = await _iAdminServices.GetLectureListByCategory(categoryId);
            return Ok(data);
        }



        [HttpGet]
        [Route("api/GetLectureById/{Id}")]
        public async Task<IActionResult> GetLectureById(int Id)
        {
            var data = await _iAdminServices.GetLectureById(Id);
            return Ok(data);
        }
        [HttpGet]
        [Route("api/DeleteLecture/{Id}")]
        public async Task<IActionResult> DeleteLecture(int Id)
        {
            var data = await _iAdminServices.DeleteLecture(Id);
            return Ok(data);
        }


        #endregion Lecture 
        #region Add Lecture Details

        [HttpGet]
        [Route("api/GetLectureDetailsList/")]
        public async Task<IActionResult> GetLectureDetailsList()
        {
            var data = await _iAdminServices.GetLectureDetailsList();
            return Ok(data);
        }


        [HttpPost]
        [Route("api/AddLectureDetails/")]
        public async Task<IActionResult> AddLectureDetails(LectureDetailVM obj)
        {
            try
            {
                string lectureDetailName = obj.Name;
                // Add Lecture
                var data = await _iAdminServices.AddLectureDetails(obj);

                if (data != null) // Assuming a successful addition returns a non-null response
                {
                    // Fetch all device tokens
                    var tokens = await _iAdminServices.GetDeviceToken();

                    if (tokens == null || !tokens.Any())
                    {
                        return Ok(new { message = "Lecture added, but no device tokens found for notification." });
                    }

                    // Use a HashSet to track unique tokens
                    var uniqueTokens = new HashSet<string>(tokens);

                    foreach (var token in uniqueTokens)
                    {
                        try
                        {
                            var notification = new NotificationModel
                            {
                                Token = token,
                                Title = "New exercises are now available in 'German Grammar'.",
                                Body = $"Lecture {lectureDetailName} available."
                            };

                            await SendNotification(notification);
                        }
                        catch (Exception ex)
                        {
                            // Log error for this token but continue with the next
                            Console.WriteLine($"Error sending notification to token {token}: {ex.Message}");
                        }
                    }

                    return Ok(
                        );
                }

                return BadRequest("Failed to add Lecture Detail.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpGet]
        [Route("api/GetLectureDetailsById/{Id}")]
        public async Task<IActionResult> GetLectureDetailsById(int Id)
        {
            var data = await _iAdminServices.GetLectureDetailsById(Id);
            return Ok(data);

        }
        [HttpGet]
        [Route("api/DeleteLectureDetails/{Id}")]
        public async Task<IActionResult> DeleteLectureDetails(int Id)
        {
            var data = await _iAdminServices.DeleteLectureDetails(Id);
            return Ok(data);
        }




        #endregion Add Lecture Details
        #endregion


        [HttpPost]
        [Route("api/DeviceToken")]
        public async Task<IActionResult> SaveDeviceToken([FromBody] DeviceTokenVM model)
        {
            // Ensure the model is valid
            if (model == null || string.IsNullOrEmpty(model.Token))
            {
                return BadRequest("Invalid device token.");
            }
            model.UserId = 1;
            // Save the device token (you may save it to the database)
            var result = await _iAdminServices.SaveDeviceToken(model);

            if (result)
            {
                return Ok("Device token saved successfully.");
            }

            return BadRequest("Failed to save device token.");
        }

        // API to retrieve the device token
        [HttpGet]
        [Route("api/getDeviceTokens")]
        public async Task<IActionResult> GetDeviceTokens()
        {
            // Retrieve all device tokens
            var tokens = await _iAdminServices.GetDeviceToken();

            if (tokens == null || !tokens.Any())
            {
                return NotFound("No device tokens found.");
            }

            return Ok(new { DeviceTokens = tokens });
        }

        //[AllowAnonymous]
        //[HttpPost]
        //[Route("api/LoginApi/")]
        //public async Task<IActionResult> Authenticate(LoginVM users)
        //{
        //    var data = await _iAdminServices.Authenticate(users);
        //    return Ok(data);
        //}


        #region Qusetion

        [HttpGet]
        [Route("api/GetQuestionList/")]
        public async Task<IActionResult> GetQuestionList()

        {
            var data = await _iAdminServices.GetQuestionList();
            return Ok(data);
        }
        [HttpGet]
        [Route("api/GetQuestionListByCategory/")]
        public async Task<IActionResult> GetQuestionListByCategory()
        {
            var data = await _iAdminServices.GetQuestionListByCategory();
            return Ok(data);
        }


        [HttpPost]
        [Route("api/AddQuestion/")]
        public async Task<IActionResult> AddQuestionr(QuestionVM obj)
        {
            var data = await _iAdminServices.AddQuestion(obj);
            return Ok(data);
        }


        [HttpGet]
        [Route("api/GetQuestionById/{Id}")]
        public async Task<IActionResult> GetQuestionById(int Id)
        {
            var data = await _iAdminServices.GetQuestionById(Id);
            return Ok(data);
        }
        [HttpGet]
        [Route("api/DeleteQuestion/{Id}")]
        public async Task<IActionResult> DeleteQuestion(int Id)
        {
            var data = await _iAdminServices.DeleteQuestion(Id);
            return Ok(data);
        }



        [HttpGet]
        [Route("api/GetQuestionDetailList/")]
        public async Task<IActionResult> GetQuestionDetailList()

        {
            var data = await _iAdminServices.GetQuestionDetailList();
            return Ok(data);
        }



        [HttpPost]
        [Route("api/AddQuestionDetail/")]
        public async Task<IActionResult> AddQuestionDetailr(QuestionDetailVM obj)
        {
            var data = await _iAdminServices.AddQuestionDetail(obj);
            return Ok(data);
        }


        [HttpGet]
        [Route("api/GetQuestionDetailById/{Id}")]
        public async Task<IActionResult> GetQuestionDetailById(int Id)
        {
            var data = await _iAdminServices.GetQuestionDetailById(Id);
            return Ok(data);
        }
        [HttpGet]
        [Route("api/DeleteQuestionDetail/{Id}")]
        public async Task<IActionResult> DeleteQuestionDetail(int Id)
        {
            var data = await _iAdminServices.DeleteQuestionDetail(Id);
            return Ok(data);
        }

        [HttpGet]
        [Route("api/GetQuickLearnList/")]
        public async Task<IActionResult> GetQuickLearnList()

        {
            var data = await _iAdminServices.GetQuickLearnList();
            return Ok(data);
        }



        [HttpPost]
        [Route("api/AddQuickLearn/")]
        public async Task<IActionResult> AddQuickLearnr(QuickLearnVM obj)
        {
            var data = await _iAdminServices.AddQuickLearn(obj);
            return Ok(data);
        }


        [HttpGet]
        [Route("api/GetQuickLearnById/{Id}")]
        public async Task<IActionResult> GetQuickLearnById(int Id)
        {
            var data = await _iAdminServices.GetQuickLearnById(Id);
            return Ok(data);
        }
        [HttpGet]
        [Route("api/DeleteQuickLearn/{Id}")]
        public async Task<IActionResult> DeleteQuickLearn(int Id)
        {
            var data = await _iAdminServices.DeleteQuickLearn(Id);
            return Ok(data);
        }

        #endregion Qusetion 
        #region ContantMaster
        [HttpGet]
        [Route("api/GetContantMasterList/")]
        public async Task<IActionResult> GetContantMasterList()

        {
            var data = await _iAdminServices.GetContantMasterList();
            return Ok(data);
        }
        [HttpPost]
        [Route("api/AddContantMaster/")]
        public async Task<IActionResult> AddContantMaster(ContentMasterVM obj)
        {
            var data = await _iAdminServices.AddContantMaster(obj);
            return Ok(data);
        }


        [HttpGet]
        [Route("api/GetContantMasterById/{Id}")]
        public async Task<IActionResult> GetContantMasterById(int Id)
        {
            var data = await _iAdminServices.GetContantMasterById(Id);
            return Ok(data);
        }
        [HttpGet]
        [Route("api/DeleteContantMaster/{Id}")]
        public async Task<IActionResult> DeleteContantMaster(int Id)
        {
            var data = await _iAdminServices.DeleteContantMaster(Id);
            return Ok(data);
        }
        #endregion ContantMaster
    }
}
