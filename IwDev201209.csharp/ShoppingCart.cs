﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace IwDev201209
{
    public class NaiveShoppingCart<TItem>
    {
        private readonly List<TItem> _items;
        private decimal _paidAmount;

        public NaiveShoppingCart()
        {
            _items = new List<TItem>();
            _paidAmount = 0;
        }

        /// Is cart paid for?
        public bool IsPaidFor { get { return _paidAmount > 0; } }

        /// Readonly list of items
        public IEnumerable<TItem> Items { get { return _items; } }

        /// add item only if not paid for
        public void AddItem(TItem item)
        {
            if (!IsPaidFor)
            {
                _items.Add(item);
            }
        }

        /// remove item only if not paid for
        public void RemoveItem(TItem item)
        {
            if (!IsPaidFor)
            {
                _items.Remove(item);
            }
        }

        /// pay for the cart
        public void Pay(decimal amount)
        {
            if (!IsPaidFor)
            {
                _paidAmount = amount;
            }
        }
    }
    // How many bugs are there in this code?




    










  
















    public class ShoppingCart<TItem>
    {

        #region ShoppingCart State classes

        /// <summary>
        /// Represents the Empty state
        /// </summary>
        public class EmptyState
        {
            public ShoppingCart<TItem> Add(TItem item)
            {
                var newItems = new[] { item };
                var newState = new ActiveState(newItems);
                return FromState(newState);
            }
        }

        /// <summary>
        /// Represents the Active state
        /// </summary>
        public class ActiveState
        {
            public ActiveState(IEnumerable<TItem> items)
            {
                Items = items;
            }

            public IEnumerable<TItem> Items { get; private set; }

            public ShoppingCart<TItem> Add(TItem item)
            {
                var newItems = new List<TItem>(Items) {item};
                var newState = new ActiveState(newItems);
                return FromState(newState);
            }

            public ShoppingCart<TItem> Remove(TItem item)
            {
                var newItems = new List<TItem>(Items);
                newItems.Remove(item);
                if (newItems.Count > 0)
                {
                    var newState = new ActiveState(newItems);
                    return FromState(newState);
                }
                else
                {
                    var newState = new EmptyState();
                    return FromState(newState);
                }
            }

            public ShoppingCart<TItem> Pay(decimal amount)
            {
                var newState = new PaidForState(Items, amount);
                return FromState(newState);
            }


        }

        /// <summary>
        /// Represents the Paid state
        /// </summary>
        public class PaidForState
        {
            public PaidForState(IEnumerable<TItem> items, decimal amount)
            {
                Items = items.ToList();
                Amount = amount;
            }

            public IEnumerable<TItem> Items { get; private set; }
            public decimal Amount { get; private set; }
        }

        #endregion ShoppingCart State classes

        //====================================
        // Execute of shopping cart proper
        //====================================

        private enum Tag { Empty, Active, PaidFor }
        private readonly Tag _tag = Tag.Empty;
        private readonly object _state;       //has to be a generic object

        /// <summary>
        /// Private ctor. Use FromState instead
        /// </summary>
        private ShoppingCart(Tag tagValue, object state)
        {
            _state = state;
            _tag = tagValue;
        }

        public static ShoppingCart<TItem> FromState(EmptyState state)
        {
            return new ShoppingCart<TItem>(Tag.Empty, state);
        }

        public static ShoppingCart<TItem> FromState(ActiveState state)
        {
            return new ShoppingCart<TItem>(Tag.Active, state);
        }

        public static ShoppingCart<TItem> FromState(PaidForState state)
        {
            return new ShoppingCart<TItem>(Tag.PaidFor, state);
        }

        /// <summary>
        /// Create a new empty cart
        /// </summary>
        public static ShoppingCart<TItem> NewCart()
        {
            var newState = new EmptyState();
            return FromState(newState);
        }

        /// <summary>
        /// Call a function for each case of the state
        /// </summary>
        /// <remarks>
        /// Forcing the caller to pass a function for each possible case means that all cases are handled at all times.
        /// </remarks>
        public TResult Do<TResult>(
            Func<EmptyState, TResult> emptyFn,
            Func<ActiveState, TResult> activeFn,
            Func<PaidForState, TResult> paidForyFn
            )
        {
            switch (_tag)
            {
                case Tag.Empty:
                    return emptyFn(_state as EmptyState);
                case Tag.Active:
                    return activeFn(_state as ActiveState);
                case Tag.PaidFor:
                    return paidForyFn(_state as PaidForState);
                default:
                    throw new InvalidOperationException(string.Format("Tag {0} not recognized", _tag));
            }
        }

        /// <summary>
        /// Do an action without a return value
        /// </summary>
        public void Do(
            Action<EmptyState> emptyFn,
            Action<ActiveState> activeFn,
            Action<PaidForState> paidForyFn
            )
        {
            //convert the Actions into Funcs by returning a dummy value
            Do(
                state => { emptyFn(state); return 0; },
                state => { activeFn(state); return 0; },
                state => { paidForyFn(state); return 0; }
                );
        }



    }

    /// <summary>
    /// Extension methods for my own personal library
    /// </summary>
    public static class ShoppingCartExtension
    {
        /// <summary>
        /// Helper method to Add
        /// </summary>
        public static ShoppingCart<TItem> Add<TItem>(this ShoppingCart<TItem> cart, TItem item)
        {
            return cart.Do(
                state => state.Add(item), //empty case
                state => state.Add(item), //active case
                state => { Console.WriteLine("ERROR: The cart is paid for and items cannot be added"); return cart; } //paid for case
            );
        }

        /// <summary>
        /// Helper method to Remove
        /// </summary>
        public static ShoppingCart<TItem> Remove<TItem>(this ShoppingCart<TItem> cart, TItem item)
        {
            return cart.Do(
                state => { Console.WriteLine("ERROR: The cart is empty and items cannot be removed"); return cart; }, //empty case
                state => state.Remove(item), //active case
                state => { Console.WriteLine("ERROR: The cart is paid for and items cannot be removed"); return cart; } //paid for case
            );
        }

        /// <summary>
        /// Helper method to Display
        /// </summary>
        public static void Display<TItem>(this ShoppingCart<TItem> cart)
        {
            cart.Do(
                state => Console.WriteLine("The cart is empty"),
                state => Console.WriteLine("The active cart contains {0} items", state.Items.Count()),
                state => Console.WriteLine("The paid cart contains {0} items. Amount paid {1}", state.Items.Count(), state.Amount)
            );
        }
    }


}

