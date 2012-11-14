﻿using System;
using System.Collections.Generic;
using Boundaries;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Interactors.Tests
{
    [TestClass]
    public class LoginInteractorTests
    {
        private Mock<ILoginPage> _page;
        private Mock<ILoginView> _view;
        private LoginInteractor _interactor;

        [TestInitialize]
        public void Setup()
        {
            _page = new Mock<ILoginPage>();
            _view = new Mock<ILoginView>();

            _interactor = new LoginInteractor(_view.Object, _page.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            _page.Verify();
            _view.Verify();
        }
        
        [TestMethod]
        public void interacts_with_webapp_to_handle_login()
        {
            var request = new LoginRequest { UserName = "user", Password = "password" };

            _interactor.Login(request);

            _page.Verify(p => p.Login(request.UserName, request.Password));
        }

        [TestMethod]
        public void when_login_fails_forwards_error_message_to_view()
        {
            _page.Setup(p => p.Login(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            _page.Setup(p => p.LastErrorMessage).Returns("error message");

            _interactor.Login(new LoginRequest());

            _view.Verify(v => v.ShowErrorMessage("error message"));
        }

        [TestMethod]
        public void when_login_succeeds_forwards_current_week_to_view()
        {
            var today = new DateTime(2012, 1, 24);
            const string nextSaturday = "1/28/2012";
            const string actualSelectedDate = "1/21/2012";

            LoginResponse expectedResponse = BuildSampleResponseWithDate(actualSelectedDate);
            LoginResponse actualResponse = null;

            SetupPageToRespondWith(expectedResponse);
            _view.Setup(v => v.SetCurrentWeek(It.IsAny<LoginResponse>())).Callback((LoginResponse r) => actualResponse = r);       
            _interactor.Clock = get_clock_fixed_on_date(today);

            _interactor.Login(new LoginRequest());

            _page.Verify(p => p.SelectCurrentWeek(nextSaturday));
            _view.Verify(v => v.SetCurrentWeek(It.IsAny<LoginResponse>()));

            actualResponse.ShouldBeEquivalentTo(expectedResponse);
        }

        private void SetupPageToRespondWith(LoginResponse expectedResponse)
        {
            _page.Setup(p => p.Login(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _page.Setup(p => p.LastErrorMessage).Returns("");
            _page.Setup(p => p.CurrentWeek).Returns(expectedResponse.CurrentWeek);
            _page.Setup(p => p.WeekDays).Returns(expectedResponse.WeekDays);
            _page.Setup(p => p.EarningCodes).Returns(expectedResponse.EarningCodes);
            _page.Setup(p => p.ContractLines).Returns(expectedResponse.ContractLines);
            _page.Setup(p => p.ContractNumbers).Returns(expectedResponse.ContractNumbers);
            _page.Setup(p => p.ActivityIDs).Returns(expectedResponse.ActivityIDs);
            _page.Setup(p => p.ProjectIDs).Returns(expectedResponse.ProjectIDs);
        }

        private LoginResponse BuildSampleResponseWithDate(string actualSelectedDate)
        {
            return new LoginResponse
                       {
                           WasSuccessful = true,
                           CurrentWeek = actualSelectedDate,
                           WeekDays = new List<string> {"Sunday", "Monday", "Tuesday"},
                           EarningCodes = new List<string> {"Billable", "Non Billable"},
                           ContractLines = new List<string> {"1", "2", "3"},
                           ContractNumbers = new List<string> {"CON0003932 Overhead"},
                           ActivityIDs = new List<string> {"ADMIN", "BENCH"},
                           ProjectIDs = new List<string> {"001234 Overhead"}
                       };
        }

        private IClock get_clock_fixed_on_date(DateTime today)
        {
            var clock = new Mock<IClock>();
            clock.Setup(c => c.Now()).Returns(today);
            return clock.Object;
        }
    }
}
