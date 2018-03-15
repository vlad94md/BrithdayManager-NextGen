using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using AutoMapper;
using BirthdayManager.Core.Constants;
using BirthdayManager.Core.Dtos;
using BirthdayManager.Core.Models;
using BirthdayManager.Persistence;

namespace BirthdayManager.Controllers.Api
{
    public class ArrangementsController : ApiController
    {
        private ApplicationDbContext _context;

        public ArrangementsController()
        {
            _context = new ApplicationDbContext();
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        [Route("api/arrangemets/{id}")]
        public IHttpActionResult GetArrangement(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid Id");

            var arrangement = _context.Arrangements
                .Include(x => x.ApplicationUser)
                .Include(x => x.Subscribers)
                .FirstOrDefault(x => x.Id == id);

            if (arrangement == null)
                return NotFound();

            var subscribersList = new List<SubscriberDto>();

            foreach (var subscription in arrangement.Subscribers)
            {
                var subscriber = _context.Users.FirstOrDefault(x => x.Id == subscription.ApplicationUserId);

                subscribersList.Add(new SubscriberDto()
                {
                    Username = subscriber.UserName,
                    Fullname = subscriber.GetFullname()
                });
            }
           
            var arrangementDto = new ArrangementDto()
            {
                Id = id,
                BirthdayManUsername = arrangement.ApplicationUser.UserName.ToLower(),
                IsComplete = arrangement.IsComplete,
                GiftDescription = arrangement.GiftDescription,
                GiftPrice = arrangement.GiftPrice,
                Birthday = arrangement.Birthday,
                SubscribersUseranmes = subscribersList
            };

            return Ok(arrangementDto);
        }


        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        [Route("api/arrangemets")]
        public IHttpActionResult SaveArrangement(ArrangementDto arrangementDto)
        {
            if (!ModelState.IsValid)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var isCreateOperation = arrangementDto.Id == 0;
            if (isCreateOperation)
            {
                var birthdayMan = _context.Users.FirstOrDefault(x => x.UserName.ToLower() == arrangementDto.BirthdayManUsername.ToLower());
                if (birthdayMan == null)
                    return BadRequest($"Birthdayman's username is invalid.");

                if (arrangementDto.SubscribersUseranmes == null)
                    return BadRequest($"At least one subscriber is required.");

                var birthday = birthdayMan.GetBirthdayForCurrentYear();
                var arrangementFromDb = _context.Arrangements
                    .FirstOrDefault(x => x.ApplicationUserId == birthdayMan.Id && x.Birthday == birthday);

                if (arrangementFromDb != null)
                    return BadRequest($"Arrangement for this users's birthday already exists.");

                var newArrangement = new Arrangement()
                {
                    ApplicationUser = birthdayMan,
                    Birthday = birthdayMan.GetBirthdayForCurrentYear(),
                    GiftDescription = arrangementDto.GiftDescription,
                    GiftPrice = arrangementDto.GiftPrice,
                    IsComplete = arrangementDto.IsComplete
                };

                _context.Arrangements.Add(newArrangement);
                _context.SaveChanges();

                foreach (var subscriberUsername in arrangementDto.SubscribersUseranmes)
                {
                    var user = _context.Users.FirstOrDefault(x => x.UserName.ToLower() == subscriberUsername.Username.ToLower());
                    if (user == null)
                        return BadRequest($"User cannot be found for this username {subscriberUsername}. Subscription creating failed.");

                    var newSubscription = new Subscription()
                    {
                        ArrangementId = newArrangement.Id,
                        ApplicationUserId = user.Id
                    };

                    _context.Subscriptions.Add(newSubscription);

                }

                _context.SaveChanges();
                return Ok(newArrangement.Id);
            }
            else
            {
                var arrangement = _context.Arrangements
                    .Include(x => x.ApplicationUser)
                    .FirstOrDefault(x => x.Id == arrangementDto.Id);

                if (arrangement == null)
                    return NotFound();

                arrangement.GiftDescription = arrangementDto.GiftDescription;
                arrangement.GiftPrice = arrangementDto.GiftPrice;
                arrangement.IsComplete = arrangementDto.IsComplete;

                if (arrangementDto.SubscribersUseranmes != null && arrangementDto.SubscribersUseranmes.Any())
                {
                    //Remove all previus subscription for full replace.
                    var subscriptions = _context.Subscriptions.Where(x => x.ArrangementId == arrangementDto.Id);
                    _context.Subscriptions.RemoveRange(subscriptions);

                    foreach (var subscriberUsername in arrangementDto.SubscribersUseranmes)
                    {
                        var user = _context.Users.FirstOrDefault(x => x.UserName.ToLower() == subscriberUsername.Username.ToLower());
                        if (user == null)
                            return BadRequest($"User cannot be found for this username {subscriberUsername}. Subscription creating failed.");

                        var newSubscription = new Subscription()
                        {
                            ArrangementId = arrangement.Id,
                            ApplicationUserId = user.Id
                        };

                        _context.Subscriptions.Add(newSubscription);
                    }
                }

                _context.SaveChanges();
                return Ok(arrangement.Id);
            }
        }
    }
}
