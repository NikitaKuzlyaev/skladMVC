namespace skladMVC.Models
{
    public class Order
    {

        public int Id { get; set; }
        public float Amount { get; set; }
        public int StatusId { get; set; }
        public string ItemsId { get; set; }
        public string AmountItems { get; set; }

        public int UserId { get; set; }
        public string Address { get; set; }
        public string Comment { get; set; }


        public static List<int> GetItemsId(string ItemsId)
        {
            List<int> items = new List<int> { };

            string phrase = ItemsId;
            string[] words = phrase.Split('#');

            foreach (var word in words)
            {
                if (word != "")
                {
                    items.Add(Int32.Parse(word));
                }
            }
            return items;
        }

        public static List<int> GetAmountItems(string AmountItems)
        {
            List<int> amount = new List<int> { };

            string phrase = AmountItems;
            string[] words = phrase.Split('#');

            foreach (var word in words)
            {
                if (word != "")
                {
                    amount.Add(Int32.Parse(word));
                }
            }
            return amount;
        }

       /* public void DeleteItem(int id)
        {
            List<int> items = GetItemsId();
            List<int> amount = GetAmountItems();

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == id)
                {
                    items.RemoveAt(i);
                    amount.RemoveAt(i);
                    break;
                }
            }
            Rewrite(items, amount);
        }*/

        /*public void AddItem(int id, int col)
        {
            List<int> items = GetItemsId();
            List<int> amount = GetAmountItems();
            items.Add(id);
            amount.Add(col);
            Rewrite(items, amount);
        }*/

        public void Rewrite(List<int> items, List<int> amount)
        {
            string newItemsId = ListToString(items);
            string newAmountItems = ListToString(amount);
            ItemsId = newItemsId;
            AmountItems = newAmountItems;
        }
 

        public static string ListToString(List<int> list)
        {
            string s = "";
            foreach (int elem in list)
            {
                s = s + elem.ToString() + "#";
            }
            return s;
        }

        /*public void Change(int id, int col)
        {
            List<int> items = GetItemsId();
            List<int> amount = GetAmountItems();

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == id)
                { 
                    amount[i] = col;
                    break;
                }
            }
            Rewrite(items, amount);
        }*/

    }
}
