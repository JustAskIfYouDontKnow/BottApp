﻿using BottApp.Database.Document;
using BottApp.Host.SimpleStateMachine;
using Xunit;

namespace BottApp.Database.Test.Document;

public class DocumentRepository: DbTestCase
{

    [Fact]
    public void CreateDocumentTest()
    {
        var user = DatabaseContainer.User.CreateUser(3435, "Hello", null).Result;
        Assert.NotNull(user);
   
        
        var document = DatabaseContainer.Document.CreateModel(user.Id, "Photo", ".jpeg", DateTime.Now, "//path//saqwe//fsa", "Описание",DocumentInPath.Votes, DocumentNomination.Biggest).Result;
        var document2 = DatabaseContainer.Document.CreateModel(user.Id, "Photo", ".jpeg", DateTime.Now, "//path//saqwe//fsa", "Описание",DocumentInPath.Votes, DocumentNomination.Fast).Result;

        Assert.NotNull(document);
        Assert.NotNull(document2);
        Assert.Equal(user.Id, document.UserId);
        Assert.Equal("Photo", document.DocumentType);

        Assert.Equal(user.UId, document.UserModel.UId);
        Assert.NotNull(document.DocumentStatisticModel);

        Assert.Equal(0, document.DocumentStatisticModel.LikeCount);


        var getDocumetById = DatabaseContainer.Document.GetOneByDocumentId(document.Id).Result;
        Assert.NotNull(getDocumetById);
        Assert.Equal(document.Path, getDocumetById.Path);
    }



    [Fact]
    public void ListDocumentsTest()
    {
        var user = DatabaseContainer.User.CreateUser(3435, "Hello", null).Result;
        Assert.NotNull(user);

        var pagination = new Pagination(0, 10);
        // var basePathCollection = new List<DocumentModel>();
        var votesPathCollection = new List<DocumentModel>();
        //
        // for (var i = 1; i <= 20; i++)
        // {
        //     basePathCollection.Add(DatabaseContainer.Document.CreateModel(user.Id, $"Base Type {i}", ".jpeg", DateTime.Now, "Base Path", DocumentInPath.Base, null).Result);
        // }
        for (var i = 1; i <= 3; i++)
        {
            votesPathCollection.Add(DatabaseContainer.Document.CreateModel(
                user.Id,
                $"Votes Type {i}",
                ".jpeg",
                DateTime.Now,
                "Vote Path",
                "Описание",
                DocumentInPath.Votes,
                DocumentNomination.Biggest).Result);
        }
        
        
        for (var i = 1; i <= 6; i++)
        {
            votesPathCollection.Add(DatabaseContainer.Document.CreateModel(
                user.Id,
                $"Votes Type {i}",
                ".jpeg",
                DateTime.Now,
                "Vote Path",
                "Описание",
                DocumentInPath.Votes,
                DocumentNomination.Fast).Result);
        }
        

        // Assert.Equal(20, basePathCollection.Count);
        Assert.Equal(9, votesPathCollection.Count);
        
        Assert.Equal(DocumentNomination.Biggest, votesPathCollection[1].DocumentNomination);
        
        // var CountInBiggest = DatabaseContainer.Document.ListDocumentsByNomination(new Pagination(0, 4), DocumentNomination.Biggest).Result;
        // var CountInFast = DatabaseContainer.Document.ListDocumentsByNomination(new Pagination(0, 6), DocumentNomination.Fast).Result;
        //
        // Assert.Equal(3, CountInBiggest.Count);
        // Assert.Equal(6, CountInFast.Count);

        // var listInBase = DatabaseContainer.Document.ListDocumentsByPath(pagination, DocumentInPath.Base).Result;
        // Assert.Equal(1000, listInBase.Count);

        //
        // var firstDocumentInVotes = DatabaseContainer.Document.GetFirstDocumentByPath(DocumentInPath.Votes).Result;
        //
        // Assert.Equal("Votes Type 1", firstDocumentInVotes.DocumentType);
        //
        // var documentCollection = DatabaseContainer.Document.ListDocumentsByPath(new Pagination(3, 2), DocumentInPath.Votes).Result;
        //
        //
        // Assert.Equal(2, documentCollection.Count);
    }
}