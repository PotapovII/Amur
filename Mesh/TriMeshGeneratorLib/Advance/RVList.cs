////---------------------------------------------------------------------------
////                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
////                         проектировщик:
////                           Потапов И.И.
////---------------------------------------------------------------------------
////                 кодировка : 11.08.2024 Потапов И.И.
////---------------------------------------------------------------------------
//namespace TriMeshGeneratorLib.Advance
//{
//    using System.Collections.Generic;
//    /// <summary>
//    /// состояние списка 
//    /// </summary>
//    public enum status
//    {
//        /// <summary>
//        /// создан массив прямой адресации
//        /// </summary>
//        valid,
//        /// <summary>
//        /// не создан массив прямой адресации
//        /// </summary>
//        notValid
//    };
//    /// <summary>
//    /// Класс Itemlist - это контроллер, который определяет функциональность связанного списка. 
//    /// Класс RVList предназначен для предоставления довольно общих возможностей для 
//    /// хранения связанных списков и доступа к коллекции объектов. Объекты должны быть 
//    /// производными от класса RVItem, чтобы содержать необходимые переменные списка. 
//    /// Приведение необходимо при получении предметов, но не при их складывании. 
//    /// Большинство операций, определенных в списке элементов, влияют только на организацию записей.
//    /// Хранилище списков не управляет памятью, ни выделением, ни освобождением самих элементов
//    /// кроме памяти массива хеширования с прямой адресацией объетов производных от класса RVItem
//    /// </summary>
//    public class RivList<T> 
//    {
//        List<T> list = new List<T> ();
//        ///// <summary>
//        ///// первый элемент в списке
//        ///// </summary>
//        //protected RVItem theFirstItem;
//        ///// <summary>
//        ///// текущий элемент в списке
//        ///// </summary>
//        //protected RVItem theCurrentItem;
//        ///// <summary>
//        ///// массив прямой адресации в состоянии статуса valid
//        ///// </summary>
//        //protected RVItem[] itemIndex;
//        /// <summary>
//        /// состояние статуса 
//        /// </summary>
//        protected status indexState;

//        public int currentIndex;
//        /// <summary>
//        /// construcor
//        /// </summary>
//        public RivList():base()
//        {
//            currentIndex = -1;
//            indexState = status.notValid;
//        }
//        /// <summary>
//        /// не удаляет элементы
//        /// does not delete items
//        /// </summary>
//        public new void Clear()
//        {
//            list.Clear();
//            currentIndex = -1;
//            indexState = status.notValid;
//        }
//        /// <summary>
//        /// вернуть указатель на ID-й элемент
//        /// return pointer to ith item
//        /// </summary>
//        /// <param name="ID">ID</param>
//        /// <returns></returns>
//        public T GetIndexItem(int ID)
//        {
//            return list[ID];
//        }
//        ///// <summary>
//        ///// выполняет для списка построение массива ссылок itemIndex  
//        ///// для прямой адресации к элементам
//        ///// </summary>
//        ///// <returns></returns>
//        //public int BuildIndex()
//        //{
//        //    int index = 0;
//        //    itemIndex = null;
//        //    if (count > 0)
//        //    {
//        //        itemIndex = new RVItem[count];
//        //        itemIndex[index] = FirstItem();
//        //        theFirstItem.Index = 0;
//        //        for (index = 1; index < count; index++)
//        //        {
//        //            itemIndex[index] = NextItem();
//        //            theCurrentItem.Index=index;
//        //        }
//        //        indexState = status.valid;
//        //    }
//        //    else
//        //    {
//        //        itemIndex = null;
//        //        indexState = status.notValid;
//        //    }
//        //    return count;
//        //}
//        /// <summary>
//        /// получить  1 элемент списка
//        /// get first item in list
//        /// </summary>
//        /// <returns></returns>
//        public T FirstItem()
//        {
//            return list[0];
//        }
//        /// <summary>
//        /// получить текущий элемент списка
//        /// get current item in list
//        /// </summary>
//        /// <returns></returns>
//        public T CurrentItem()
//        {
//            return list[currentIndex];
//        }
//        /// <summary>
//        /// установить текущий элемент в списке
//        /// set current item in list
//        /// </summary>
//        /// <param name="itemP"></param>
//        /// <returns></returns>
//        public RVItem SetCurrentItem(RVItem itemP)
//        {
//            if (itemP != null)
//                theCurrentItem = itemP;
//            else
//                theCurrentItem = theFirstItem;
//            return theCurrentItem;
//        }
//        /// <summary>
//        /// получить следующий элемент в списках
//        /// get next item in lists
//        /// </summary>
//        /// <returns></returns>
//        public RVItem NextItem()
//        {
//            if (count > 0)
//            {
//                if (theCurrentItem.Next != null)
//                {
//                    theCurrentItem = theCurrentItem.Next;
//                    return (theCurrentItem);
//                }
//                else
//                    return (null);
//            }
//            else
//                return (null);
//        }
//        public RVItem LastItem()
//        {
//            if (count > 0)
//            {
//                while (theCurrentItem.Next != null)
//                {
//                    theCurrentItem = theCurrentItem.Next;
//                }
//            }
//            return (theCurrentItem);
//        }
//        /// <summary>
//        /// get item with n = ID
//        /// </summary>
//        /// <param ID="ID"></param>
//        /// <returns></returns>
//        public RVItem Get(int ID)
//        {
//            if (count > 0)
//            {
//                theCurrentItem = theFirstItem;
//                while (theCurrentItem.ID != ID)
//                {
//                    if (theCurrentItem.Next != null)
//                        theCurrentItem = theCurrentItem.Next;
//                    else
//                        return null;
//                }
//                return theCurrentItem;
//            }
//            else
//                return null;
//        }
//        /// <summary>
//        /// добавить новый элемент в конец списка
//        /// add new item to end of list
//        /// </summary>
//        /// <param name="theNewItem"></param>
//        /// <returns></returns>
//        public RVItem Add(RVItem theNewItem)
//        {
//            if (count > 0)
//            {
//                while (theCurrentItem.Next != null)
//                {
//                    theCurrentItem = theCurrentItem.Next;
//                }
//            }
//            return (Insert(theNewItem));
//        }
//        /// <summary>
//        /// вставить новый элемент после текущего элемента
//        /// insert new item after current item
//        /// </summary>
//        /// <param name="theNewItem"></param>
//        /// <returns></returns>
//        public RVItem Insert(RVItem theNewItem)
//        {
//            if (count > 0)
//            {
//                RVItem theNextItem = theCurrentItem.Next;
//                theNewItem.Next = theNextItem;
//                theNewItem.Prev = theCurrentItem;
//                if (theNextItem != null)
//                    theNextItem.Prev = theNewItem;
//                theCurrentItem.Next = theNewItem;
//                theCurrentItem = theNewItem;
//            }
//            else
//            {
//                theNewItem.Next = null;
//                theNewItem.Prev = null;
//                theFirstItem = theNewItem;
//                theCurrentItem = theNewItem;
//            }
//            count += 1;
//            indexState = status.notValid;

//            return (theNewItem);
//        }
//        /// <summary>
//        /// поставить новый элемент в начало списка
//        /// put new item at start of list
//        /// </summary>
//        /// <param name="theNewItem"></param>
//        /// <returns></returns>
//        public RVItem Push(RVItem theNewItem)
//        {
//            if (count > 0)
//            {
//                theNewItem.Next = theFirstItem;
//                theNewItem.Prev = null;
//                theFirstItem = theNewItem;
//                theCurrentItem = theNewItem;
//            }
//            else
//            {
//                theNewItem.Next = null;
//                theNewItem.Prev = null;
//                theFirstItem = theNewItem;
//                theCurrentItem = theNewItem;
//            }
//            count += 1;
//            indexState = status.notValid;

//            return (theNewItem);
//        }
//        /// <summary>
//        /// take (remove) item from start of list
//        /// взять (удалить) элемент из начала списка
//        /// </summary>
//        /// <returns></returns>
//        public RVItem Pop()
//        {
//            RVItem popItemP;

//            if (count > 0)
//            {
//                popItemP = theFirstItem;
//                theFirstItem = popItemP.Next;
//                if (theFirstItem != null)
//                    theFirstItem.Prev = null;
//                theCurrentItem = theFirstItem;
//                count -= 1;
//                indexState = status.notValid;

//                return popItemP;
//            }
//            else
//                return null;
//        }
//        /// <summary>
//        /// concatenate another list to end of list
//        /// объединить другой список в конец списка
//        /// </summary>
//        /// <param name="otherList"></param>
//        public void AddRange(RVList otherList)
//        {

//            if (count > 0)
//            {
//                while (theCurrentItem.Next != null)
//                {
//                    theCurrentItem = theCurrentItem.Next;
//                }
//                RVItem otherFirstItem = otherList.FirstItem();
//                theCurrentItem.Next = otherFirstItem;
//                if (otherFirstItem != null)
//                    otherList.FirstItem().Prev = theCurrentItem;
//                theCurrentItem = theFirstItem;
//            }
//            else
//            {
//                theFirstItem = otherList.FirstItem();
//                theCurrentItem = theFirstItem;
//            }
//            count += otherList.Count;
//            indexState = status.notValid;
//        }
//        /// <summary>
//        /// remove current item, next becomes current
//        /// </summary>
//        /// <returns></returns>
//        public RVItem RemoveCurrentItem()
//        {
//            RVItem prevItem;

//            prevItem = theFirstItem;

//            if (theFirstItem != null)
//            {
//                if (theCurrentItem != theFirstItem)
//                {
//                    while (prevItem.Next != theCurrentItem)
//                    {
//                        prevItem = prevItem.Next;
//                        if (prevItem == null)
//                        {
//                            return theCurrentItem;
//                        }
//                    }
//                    prevItem.Next = theCurrentItem.Next;
//                    if (prevItem.Next != null)
//                        theCurrentItem = prevItem.Next;
//                    else
//                        theCurrentItem = prevItem;
//                }
//                else
//                {
//                    theFirstItem = theCurrentItem.Next;
//                    theCurrentItem = theFirstItem;
//                }
//                count -= 1;
//            }
//            indexState = status.notValid;
//            return (theCurrentItem);
//        }
//        /// <summary>
//        /// delete item pointed at, return CurrentItem
//        /// </summary>
//        /// <param name="iP"></param>
//        /// <returns></returns>
//        public RVItem Remove(RVItem iP)
//        {
//            RVItem prevItem = iP.Prev;
//            RVItem NextItem = iP.Next;

//            if (theFirstItem == null)
//                return null;

//            if (iP == theFirstItem)
//            {
//                theFirstItem = NextItem;
//                if (NextItem != null)
//                    NextItem.Prev = null;
//                count -= 1;
//                indexState = status.notValid;
//                if (iP == theCurrentItem)
//                {
//                    theCurrentItem = theFirstItem;
//                }
//                return theCurrentItem;
//            }

//            if (iP == prevItem.Next)
//            {
//                prevItem.Next = NextItem;
//                if (NextItem != null)
//                    NextItem.Prev = prevItem;
//                count -= 1;
//                indexState = status.notValid;
//                if (iP == theCurrentItem)
//                    theCurrentItem = prevItem;
//            }
//            return theCurrentItem;
//        }
//    }
//}
