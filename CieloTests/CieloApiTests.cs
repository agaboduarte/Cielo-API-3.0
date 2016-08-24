﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cielo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cielo.Models;
using Cielo.Configurations;

namespace Cielo.Tests
{
    [TestClass()]
    public class CieloApiTests
    {
        private CieloApi api;
        private DateTime validExpirationDate;
        private DateTime invalidExpirationDate;

        [TestInitialize]
        public void ConfigEnvironment()
        {
            api = new CieloApi(CieloEnvironment.Sandbox, Merchant.Sandbox);
            validExpirationDate = new DateTime(DateTime.Now.Year + 1, 12, 1);
            invalidExpirationDate = new DateTime(DateTime.Now.Year - 1, 12, 1);
        }

        [TestMethod()]
        public void CriaUmaTransacaoAutorizadaSemCapturaResultadoAutorizada()
        {
            var customer = new Customer(name: "Fulano da Silva");

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.Authorized1, 
                holder: "Teste Holder", 
                expirationDate: validExpirationDate, 
                securityCode: "123", 
                brand: Enums.CardBrand.Visa);

            var payment = new Payment(
                amount: 15700, 
                currency: Enums.Currency.BRL, 
                installments: 1, 
                capture: false, 
                softDescriptor: ".Net Test Project", 
                creditCard: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(), 
                customer: customer, 
                payment: payment);

            var returnTransaction = api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsTrue(returnTransaction.Payment.Status == Enums.Status.Authorized, "Transação não foi autorizada");
        }

        [TestMethod()]
        public void CriaUmaTransacaoComCapturaAutorizadaComParcelaMenorQueCincoReaisResultadoNaoAutorizada()
        {
            var merchantOrderId = new Random().Next();
            var customer = new Customer("Fulano da Silva");
            var creditCard = new CreditCard(SandboxCreditCard.Authorized1, "Teste Holder", new DateTime(DateTime.Now.Year + 1, 12, 1), "123", Enums.CardBrand.Visa);
            var payment = new Payment(1000, Enums.Currency.BRL, 10, true, ".Net Test Project", creditCard);
            var transaction = new Transaction(merchantOrderId.ToString(), customer, payment);

            var returnTransaction = api.CreateTransaction(Guid.NewGuid(), transaction);

            // Email enviado sobre o problema.
            Assert.IsTrue(returnTransaction.Payment.Status == Enums.Status.Denied, "Transação não foi negada");
        }

        [TestMethod()]
        public void CriaUmaTransacaoAutorizadaComCapturaResultadoPagamentoConfirmado()
        {
            var merchantOrderId = new Random().Next();
            var customer = new Customer("Fulano da Silva");
            var creditCard = new CreditCard(SandboxCreditCard.Authorized1, "Teste Holder", new DateTime(DateTime.Now.Year + 1, 12, 1), "123", Enums.CardBrand.Visa);
            var payment = new Payment(15700, Enums.Currency.BRL, 1, true, ".Net Test Project", creditCard);
            var transaction = new Transaction(merchantOrderId.ToString(), customer, payment);

            var returnTransaction = api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsTrue(returnTransaction.Payment.Status == Enums.Status.PaymentConfirmed, "Transação não teve pagamento confirmado");
        }

        [TestMethod()]
        public void CriaUmaTransacaoComCartaoNaoAutorizadoResultadoNaoAutorizar()
        {
            var merchantOrderId = new Random().Next();
            var customer = new Customer("Fulano da Silva");
            var creditCard = new CreditCard(SandboxCreditCard.NotAuthorized, "Teste Holder", new DateTime(DateTime.Now.Year + 1, 12, 1), "123", Enums.CardBrand.Visa);
            var payment = new Payment(15700, Enums.Currency.BRL, 1, true, ".Net Test Project", creditCard);
            var transaction = new Transaction(merchantOrderId.ToString(), customer, payment);

            var returnTransaction = api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsTrue(returnTransaction.Payment.Status == Enums.Status.Denied, "Transação não foi negada");
        }

        [TestMethod()]
        public void CriaUmaTransacaoComCartaoBloqueadoResultadoNaoAutorizar()
        {
            var merchantOrderId = new Random().Next();
            var customer = new Customer("Fulano da Silva");
            var creditCard = new CreditCard(SandboxCreditCard.NotAuthorizedCardBlocked, "Teste Holder", new DateTime(DateTime.Now.Year + 1, 12, 1), "123", Enums.CardBrand.Visa);
            var payment = new Payment(15700, Enums.Currency.BRL, 1, true, ".Net Test Project", creditCard);
            var transaction = new Transaction(merchantOrderId.ToString(), customer, payment);

            var returnTransaction = api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsTrue(returnTransaction.Payment.Status == Enums.Status.Denied, "Transação não foi negada");
        }

        [TestMethod()]
        public void CriaUmaTransacaoComCartaoCanceladoResultadoNaoAutorizar()
        {
            var merchantOrderId = new Random().Next();
            var customer = new Customer("Fulano da Silva");
            var creditCard = new CreditCard(SandboxCreditCard.NotAuthorizedCardCanceled, "Teste Holder", new DateTime(DateTime.Now.Year + 1, 12, 1), "123", Enums.CardBrand.Visa);
            var payment = new Payment(15700, Enums.Currency.BRL, 1, true, ".Net Test Project", creditCard);
            var transaction = new Transaction(merchantOrderId.ToString(), customer, payment);

            var returnTransaction = api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsTrue(returnTransaction.Payment.Status == Enums.Status.Denied, "Transação não foi negada");
        }

        [TestMethod()]
        public void CriaUmaTransacaoComCartaoExpiradoResultadoNaoAutorizar()
        {
            var merchantOrderId = new Random().Next();
            var customer = new Customer("Fulano da Silva");
            var creditCard = new CreditCard(SandboxCreditCard.NotAuthorizedCardExpired, "Teste Holder", new DateTime(DateTime.Now.Year + 1, 12, 1), "123", Enums.CardBrand.Visa);
            var payment = new Payment(15700, Enums.Currency.BRL, 1, true, ".Net Test Project", creditCard);
            var transaction = new Transaction(merchantOrderId.ToString(), customer, payment);

            var returnTransaction = api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsTrue(returnTransaction.Payment.Status == Enums.Status.Denied, "Transação não foi negada");
        }

        [TestMethod()]
        public void CriaUmaTransacaoComCartaoComProblemasResultadoNaoAutorizar()
        {
            var merchantOrderId = new Random().Next();
            var customer = new Customer("Fulano da Silva");
            var creditCard = new CreditCard(SandboxCreditCard.NotAuthorizedCardProblems, "Teste Holder", new DateTime(DateTime.Now.Year + 1, 12, 1), "123", Enums.CardBrand.Visa);
            var payment = new Payment(15700, Enums.Currency.BRL, 1, true, ".Net Test Project", creditCard);
            var transaction = new Transaction(merchantOrderId.ToString(), customer, payment);

            var returnTransaction = api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsTrue(returnTransaction.Payment.Status == Enums.Status.Denied, "Transação não foi negada");
        }

        [TestMethod()]
        public void CriaUmaTransacaoComCartaoDeTimeOutInternoCieloResultadoNaoAutorizado()
        {
            var merchantOrderId = new Random().Next();
            var customer = new Customer("Fulano da Silva");
            var creditCard = new CreditCard(SandboxCreditCard.NotAuthorizedTimeOut, "Teste Holder", new DateTime(DateTime.Now.Year + 1, 12, 1), "123", Enums.CardBrand.Visa);
            var payment = new Payment(15700, Enums.Currency.BRL, 1, true, ".Net Test Project", creditCard);
            var transaction = new Transaction(merchantOrderId.ToString(), customer, payment);

            var returnTransaction = api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsTrue(returnTransaction.Payment.Status == Enums.Status.Denied, "Transação não foi negada");
        }
    }
}