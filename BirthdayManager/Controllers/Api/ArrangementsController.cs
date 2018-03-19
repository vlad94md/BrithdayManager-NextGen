using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using AutoMapper;
using BirthdayManager.Core.Constants;
using BirthdayManager.Core.Dtos;
using BirthdayManager.Core.Enums;
using BirthdayManager.Core.Models;
using BirthdayManager.Persistence;

namespace BirthdayManager.Controllers.Api
{
    public class ArrangementsController : ApiController
    {
        private ApplicationDbContext _context;
        private decimal _fixedBirthdayFee = 60;

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
                return BadRequest();

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

                if (arrangement.IsComplete)
                    return BadRequest("You can't make any changes after card is completed.");

                arrangement.GiftDescription = arrangementDto.GiftDescription;
                arrangement.GiftPrice = arrangementDto.GiftPrice;

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

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        [Route("api/arrangemets/finish/{id}")]
        public IHttpActionResult FinishArrangement(int id)
        {
            if (id <= 0)
                return BadRequest("Id is invalid.");

            var arrangement = _context.Arrangements
                .Include(x => x.Subscribers)
                .Include(x => x.ApplicationUser)
                .FirstOrDefault(x => x.Id == id);

            if (arrangement == null)
                return NotFound();

            if (arrangement.IsComplete)
                return BadRequest("Arrangement is already marked as completed.");

            if (!arrangement.Subscribers.Any())
                return BadRequest("Arrangement doesnt have any subscribers.");

            if (arrangement.GiftPrice <= 0)
                return BadRequest("Gift price should be more than zero.");

            arrangement.IsComplete = true;
            var subsribersIds = arrangement.Subscribers.Select(x => x.ApplicationUserId).ToList();

            var subscribers = _context.Users.Where(x => subsribersIds.Contains(x.Id)).ToList();

            var totalBudgetCollected = subscribers.Count * _fixedBirthdayFee;
            var calculatedFeePerPerson = _fixedBirthdayFee - (totalBudgetCollected - arrangement.GiftPrice) / subscribers.Count;

            if (calculatedFeePerPerson <= 0)
                return BadRequest("Calculated amount per person cannot be negative. Change the gift price.");

            foreach (var user in subscribers)
            {
                _context.MoneyTransactions.Add(new MoneyTransaction()
                {
                    ApplicationUserId = user.Id,
                    Date = DateTime.Now,
                    Type = TransactionType.Withdraw,
                    Description = $"{arrangement.ApplicationUser.GetFullname()} birthday finished.",
                    Amount = -calculatedFeePerPerson
                });
                user.Balance -= calculatedFeePerPerson;
            }

            _context.SaveChanges();
            return Ok(true);
        }
    }
}
