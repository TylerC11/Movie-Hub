using Microsoft.VisualStudio.TestTools.UnitTesting;
using Movie_Hub.Models;
using Movie_Hub.ViewModels;

namespace Movie_Hub.Tests
{
    internal static class TitleFactory
    {
        public static Title Make(string titleId, string name, short year = 2020, decimal rating = 7.5m)
            => new Title
            {
                TitleId = titleId,
                TitleType = "movie",
                PrimaryTitle = name,
                StartYear = year,
                Rating = new Rating { TitleId = titleId, AverageRating = rating, NumVotes = 10_000 }
            };
    }

    [TestClass]
    public class FavouritesViewModelTests
    {
        // ── Initial state ──────────────────────────────────────────────────

        [TestMethod]
        public void OnConstruction_FavouritesCollectionIsEmpty()
        {
            var vm = new FavouritesViewModel();
            Assert.AreEqual(0, vm.Favourites.Count);
        }

        [TestMethod]
        public void OnConstruction_HasFavouritesIsFalse()
        {
            var vm = new FavouritesViewModel();
            Assert.IsFalse(vm.HasFavourites);
        }

        [TestMethod]
        public void OnConstruction_StatusMessageMentionsNothingSaved()
        {
            var vm = new FavouritesViewModel();
            StringAssert.Contains(vm.StatusMessage, "No favourites");
        }

        // ── Add ────────────────────────────────────────────────────────────

        [TestMethod]
        public void Add_ValidTitle_IncreasesCount()
        {
            var vm = new FavouritesViewModel();
            bool added = vm.Add(TitleFactory.Make("tt0001", "Inception"));
            Assert.IsTrue(added);
            Assert.AreEqual(1, vm.Favourites.Count);
        }

        [TestMethod]
        public void Add_SameTitle_NotAddedTwice()
        {
            var vm = new FavouritesViewModel();
            var title = TitleFactory.Make("tt0001", "Inception");
            vm.Add(title);
            bool addedAgain = vm.Add(title);
            Assert.IsFalse(addedAgain);
            Assert.AreEqual(1, vm.Favourites.Count);
        }

        [TestMethod]
        public void Add_NullTitle_ReturnsFalseAndCollectionUnchanged()
        {
            var vm = new FavouritesViewModel();
            Assert.IsFalse(vm.Add(null));
            Assert.AreEqual(0, vm.Favourites.Count);
        }

        [TestMethod]
        public void Add_MultipleDifferentTitles_AllPresent()
        {
            var vm = new FavouritesViewModel();
            vm.Add(TitleFactory.Make("tt0001", "Inception"));
            vm.Add(TitleFactory.Make("tt0002", "The Dark Knight"));
            vm.Add(TitleFactory.Make("tt0003", "Interstellar"));
            Assert.AreEqual(3, vm.Favourites.Count);
        }

        // ── Remove ─────────────────────────────────────────────────────────

        [TestMethod]
        public void Remove_ExistingTitle_DecreasesCount()
        {
            var vm = new FavouritesViewModel();
            var title = TitleFactory.Make("tt0001", "Inception");
            vm.Add(title);
            Assert.IsTrue(vm.Remove(title));
            Assert.AreEqual(0, vm.Favourites.Count);
        }

        [TestMethod]
        public void Remove_TitleNotInList_ReturnsFalse()
        {
            var vm = new FavouritesViewModel();
            Assert.IsFalse(vm.Remove(TitleFactory.Make("tt9999", "Unknown")));
        }

        [TestMethod]
        public void Remove_NullTitle_ReturnsFalse()
        {
            var vm = new FavouritesViewModel();
            Assert.IsFalse(vm.Remove(null));
        }

        [TestMethod]
        public void Remove_ClearsSelectedFavouriteIfItMatchesRemovedTitle()
        {
            var vm = new FavouritesViewModel();
            var title = TitleFactory.Make("tt0001", "Inception");
            vm.Add(title);
            vm.SelectedFavourite = title;
            vm.Remove(title);
            Assert.IsNull(vm.SelectedFavourite);
        }

        // ── IsFavourite ────────────────────────────────────────────────────

        [TestMethod]
        public void IsFavourite_TitleInList_ReturnsTrue()
        {
            var vm = new FavouritesViewModel();
            var title = TitleFactory.Make("tt0001", "Inception");
            vm.Add(title);
            Assert.IsTrue(vm.IsFavourite(title));
        }

        [TestMethod]
        public void IsFavourite_TitleNotInList_ReturnsFalse()
        {
            var vm = new FavouritesViewModel();
            Assert.IsFalse(vm.IsFavourite(TitleFactory.Make("tt0001", "Inception")));
        }

        [TestMethod]
        public void IsFavourite_NullTitle_ReturnsFalse()
        {
            var vm = new FavouritesViewModel();
            Assert.IsFalse(vm.IsFavourite(null));
        }

        // ── HasFavourites ──────────────────────────────────────────────────

        [TestMethod]
        public void HasFavourites_AfterAdd_BecomesTrue()
        {
            var vm = new FavouritesViewModel();
            vm.Add(TitleFactory.Make("tt0001", "Inception"));
            Assert.IsTrue(vm.HasFavourites);
        }

        [TestMethod]
        public void HasFavourites_AfterRemovingLastItem_BecomesFalse()
        {
            var vm = new FavouritesViewModel();
            var title = TitleFactory.Make("tt0001", "Inception");
            vm.Add(title);
            vm.Remove(title);
            Assert.IsFalse(vm.HasFavourites);
        }

        // ── Clear All ──────────────────────────────────────────────────────

        [TestMethod]
        public void ClearAllCommand_RemovesAllItems()
        {
            var vm = new FavouritesViewModel();
            vm.Add(TitleFactory.Make("tt0001", "Inception"));
            vm.Add(TitleFactory.Make("tt0002", "The Dark Knight"));
            vm.ClearAllCommand.Execute(null);
            Assert.AreEqual(0, vm.Favourites.Count);
        }

        [TestMethod]
        public void ClearAllCommand_CanExecute_FalseWhenEmpty()
        {
            var vm = new FavouritesViewModel();
            Assert.IsFalse(vm.ClearAllCommand.CanExecute(null));
        }

        [TestMethod]
        public void ClearAllCommand_CanExecute_TrueAfterAdd()
        {
            var vm = new FavouritesViewModel();
            vm.Add(TitleFactory.Make("tt0001", "Inception"));
            Assert.IsTrue(vm.ClearAllCommand.CanExecute(null));
        }

        // ── Status message ─────────────────────────────────────────────────

        [TestMethod]
        public void StatusMessage_OneItem_UsesSingularForm()
        {
            var vm = new FavouritesViewModel();
            vm.Add(TitleFactory.Make("tt0001", "Inception"));
            Assert.AreEqual("1 movie saved.", vm.StatusMessage);
        }

        [TestMethod]
        public void StatusMessage_MultipleItems_IncludesCount()
        {
            var vm = new FavouritesViewModel();
            vm.Add(TitleFactory.Make("tt0001", "Inception"));
            vm.Add(TitleFactory.Make("tt0002", "The Dark Knight"));
            Assert.AreEqual("2 movies saved.", vm.StatusMessage);
        }

        [TestMethod]
        public void StatusMessage_AfterClear_ReturnsEmptyMessage()
        {
            var vm = new FavouritesViewModel();
            vm.Add(TitleFactory.Make("tt0001", "Inception"));
            vm.ClearAllCommand.Execute(null);
            StringAssert.Contains(vm.StatusMessage, "No favourites");
        }

        // ── PropertyChanged ────────────────────────────────────────────────

        [TestMethod]
        public void SelectedFavourite_Set_FiresPropertyChanged()
        {
            var vm = new FavouritesViewModel();
            var title = TitleFactory.Make("tt0001", "Inception");
            vm.Add(title);
            bool fired = false;
            vm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(vm.SelectedFavourite)) fired = true; };
            vm.SelectedFavourite = title;
            Assert.IsTrue(fired);
        }

        [TestMethod]
        public void StatusMessage_ChangesAfterAdd_FiresPropertyChanged()
        {
            var vm = new FavouritesViewModel();
            bool fired = false;
            vm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(vm.StatusMessage)) fired = true; };
            vm.Add(TitleFactory.Make("tt0001", "Inception"));
            Assert.IsTrue(fired);
        }

        // ── RemoveCommand with parameter ───────────────────────────────────

        [TestMethod]
        public void RemoveCommand_WithParameter_RemovesCorrectTitle()
        {
            var vm = new FavouritesViewModel();
            var t1 = TitleFactory.Make("tt0001", "Inception");
            var t2 = TitleFactory.Make("tt0002", "The Dark Knight");
            vm.Add(t1);
            vm.Add(t2);
            vm.RemoveCommand.Execute(t1);
            Assert.AreEqual(1, vm.Favourites.Count);
            Assert.AreEqual("tt0002", vm.Favourites[0].TitleId);
        }
    }
}
