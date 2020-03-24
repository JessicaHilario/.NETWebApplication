using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.IO;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace MediKeeper_Items.Controllers
{
    public class HomeController : Controller
    {
        static string connStr =
            "server=medikeeper-project.ccgkrwo73nam.us-east-2.rds.amazonaws.com;user=admin;database=items;port=3306;password=";
        MySqlConnection conn = new MySqlConnection(connStr);
        
        public ActionResult Index()
        {
            string htmlOrig = "<div style=\"text-align: center; line-heght: 8px;\"><h3 style=\"text-align: center\">Item Name ---- Cost</h3><br>";
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();

                // Check if there is nothing in the database first
                string sql = "SELECT COUNT(*) FROM items";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                Object count = cmd.ExecuteScalar();
                if (Convert.ToInt32(count) == 0)
                {
                    Console.WriteLine("Entering readFile function...");
                    readFile();
                    Console.WriteLine("Done with readFile function...");
                }
                else
                {
                    
                    sql = "SELECT ITEM_NAME,COST FROM items";
                    cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader  rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        htmlOrig += ("<span style=\"display: inline-block; vetical-align: middle; line-height: normal; font-size: 15px;\">" + rdr[0].ToString()+" ---- "+rdr[1].ToString()+"</span><br>");
                    }
                    htmlOrig += "</div>";

                    rdr.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            conn.Close();
            Console.WriteLine("Done.");
            
            
            ViewBag.htmlOrig = htmlOrig;
            return View();
        }

        [HttpGet]
        public ActionResult itemPrice()
        {
            string htmlOrig = "<div style=\"text-align: center; line-heght: 8px;\"><h2 style=\"text-align: center\">Item Name</h2><br>";
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();

                // Check if there is nothing in the database first
                string sql = "SELECT COUNT(*) FROM items";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                Object count = cmd.ExecuteScalar();
                if (Convert.ToInt32(count) == 0)
                {
                    Console.WriteLine("Entering readFile function...");
                    readFile();
                    Console.WriteLine("Done with readFile function...");
                }
                else
                {
                    sql = "SELECT DISTINCT(ITEM_NAME) FROM items";
                    cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader  rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        htmlOrig += ("<span style=\"display: inline-block; vetical-align: middle; line-height: normal; font-size: 15px;\">"+rdr[0].ToString()+"</span><br>");
                    }
                    htmlOrig += "</div>";

                    rdr.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            conn.Close();
            ViewBag.htmlOrig = htmlOrig;
            return View();
        }

        [HttpPost]
        public ActionResult itemPrice(string itemName)
        {
            string name = itemName.ToUpper();
            string htmlOrig = "<div style=\"text-align: center; line-heght: 8px;\"><h2 style=\"text-align: center\">Item Name</h2><br>";
            Console.WriteLine(name);
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();
                // Check if there is nothing in the database first
                string sql = "SELECT COUNT(*) FROM items";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                Object count = cmd.ExecuteScalar();
                MySqlDataReader rdr;
                if (Convert.ToInt32(count) == 0)
                {
                    Console.WriteLine("Entering readFile function...");
                    readFile();
                    Console.WriteLine("Done with readFile function...");
                }
                else
                {
                    sql = "SELECT DISTINCT(ITEM_NAME) FROM items";
                    cmd = new MySqlCommand(sql, conn);
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        htmlOrig += ("<span style=\"display: inline-block; vetical-align: middle; line-height: normal; font-size: 15px;\">" + rdr[0].ToString()+"</span><br>");
                    }
                    htmlOrig += "</div>";
                    rdr.Close();
                }
                
                // Get the names of the items and check if what the user gave a valid item
                sql = "SELECT DISTINCT(ITEM_NAME) FROM items";
                cmd = new MySqlCommand(sql, conn);
                rdr = cmd.ExecuteReader();
                Boolean isFound = false;
                while (rdr.Read())
                {
                    if (rdr[0].Equals(name))
                    {
                        isFound = true;
                        break;
                    }
                }

                rdr.Close();
                // If the user did not give a valid item name
                if (!isFound)
                {
                    ViewBag.cost = "Not A Valid Item";
                }
                else
                {
                    // SQL cquery to insert the values from the text file to the DB
                    sql = "SELECT MAX(COST) FROM items WHERE ITEM_NAME = @item";
                    cmd = new MySqlCommand(sql, conn);

                    // Give each parameter a value
                    cmd.Parameters.AddWithValue("@item", name);
                    Object maxCost = cmd.ExecuteScalar();

                    ViewBag.cost = maxCost;
                    
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            ViewBag.htmlOrig = htmlOrig;
            return View();
        }
        
        public ActionResult itemsPrice()
        {
            string htmlOrig = "<div style=\"text-align: center; line-heght: 8px;\"><h3 style=\"text-align: center\">Item Name ---- Max Cost</h3><br>";
            try
            {
                conn.Open();
                // Get the max cost for each item
                string sql = "SELECT ITEM_NAME, MAX(COST) FROM items GROUP BY ITEM_NAME";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    htmlOrig += ("<span style=\"display: inline-block; vetical-align: middle; line-height: normal; font-size: 15px;\">" + rdr[0].ToString()+" ---- "+rdr[1].ToString()+"</span><br>");
                }
                htmlOrig += "</div>";
                rdr.Close();
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.ToString());
            }

            ViewBag.html = htmlOrig;
            
            return View();
        }
        
        // Read from the items file
        public void readFile()
        {
            string filePath = (string.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory, "\\App_Data\\items.txt"));
            
            if (System.IO.File.Exists(filePath))
            {
                int count = 1;
                string[] lines = System.IO.File.ReadAllLines(filePath); // Store each line in the text file in the array
                foreach (string line in lines)
                {
                    if (count == 1) // Ignore the header
                    {
                        count++;
                        continue;
                    }
                    else
                    {
                        // Split each line at the commas
                        string[] parts = line.Split(',');
                        
                        // SQL cquery to insert the values from the text file to the DB
                        string sql = "INSERT INTO items (ID,ITEM_NAME,COST) VALUES (@ID, @ITEM_NAME, @COST)";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        
                        // Give each parameter a value
                        cmd.Parameters.AddWithValue("@ID", Int32.Parse(parts[0]));
                        cmd.Parameters.AddWithValue("@ITEM_NAME", parts[1]);
                        cmd.Parameters.AddWithValue("@COST", Int32.Parse(parts[2]));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}