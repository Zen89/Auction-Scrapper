using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
//using OpenQA.Selenium.Edge;
using System.Collections.Generic;
using System;
using System.Threading;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Interactions;
//using OpenQA.Selenium.DevTools.Console;
//using OpenQA.Selenium.DevTools.Network;
//using OpenQA.Selenium.DevTools.Page;

class Auction_Scrapper
{
    static void Main(string[] args)
    {
        // Set configuration Web driver
        var chromeOptions = new ChromeOptions();
        // chromeOptions.AddArgument("--headless"); // Start the browser in interfaceless mode
        // chromeOptions.AddArgument("--disable-gpu"); // Disable GPU support to reduce the risk of being detected as a robot
        chromeOptions.AddArgument("--user-agent=Mozilla/5.0 (Linux; Android 7.0; SM-G930V Build/NRD90M) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.125 Mobile Safari/537.36");

        // Create Web driver
        IWebDriver driver = new ChromeDriver(chromeOptions);

        int milliseconds = 2500;
        Thread.Sleep(milliseconds);

        string websiteAddress = "https://www.alledrogo.pl/";
        string cutOutText = "dro";
        int startIndex = websiteAddress.IndexOf(cutOutText);
        int length = cutOutText.Length;
        string result = websiteAddress.Remove(startIndex, length);
        int insertIndex = result.IndexOf("g") + 1;
        string insertText = "r";
        result = result.Insert(insertIndex, insertText);
        websiteAddress = result;

        // Open "alledrogo.pl" website
        driver.Navigate().GoToUrl(websiteAddress);

        Thread.Sleep(milliseconds);

        // Click reject rodo button
        IWebElement rejectButton = driver.FindElement(By.CssSelector("button[data-role='reject-rodo']"));
        rejectButton.Click();

        Thread.Sleep(milliseconds);


        Console.WriteLine("Wprowadź produkt, którego dane chcesz wyszukać: ");
        string searchProduct = Console.ReadLine();

        // Search searching window and set user product
        IWebElement searchBox = driver.FindElement(By.Name("string"));
        searchBox.SendKeys(searchProduct);

        Thread.Sleep(milliseconds);

        // Click search button
        IWebElement searchButton = driver.FindElement(By.CssSelector("button[data-role='search-button']"));
        searchButton.Click();

        Thread.Sleep(milliseconds);

        for (int i = 0; i < 100; i++) //TODO reading the number of pages available for viewing
        {
            Console.WriteLine($"Numer przeglądanej strony: {i + 1} ");

            // Wait to upload website
            IWebElement searchResults = driver.FindElement(By.CssSelector("div[data-box-name='items-v3']"));
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(driver => searchResults.Displayed);

            // Download auction numbers, auction titles and "buy now" prices
            IReadOnlyCollection<IWebElement> itemBoxes = driver.FindElements(By.CssSelector("article[data-item=true]"));
            foreach (IWebElement itemBox in itemBoxes)
            {
                string auctionNumber = itemBox.GetAttribute("data-analytics-view-value");
                string buyNowPriceAndName = itemBox.GetAttribute("aria-label");
                //string buyNowPrice = itemBox.FindElement(By.CssSelector("div[data-role='price'] > span")).Text;

                Console.WriteLine($"Numer aukcji: {auctionNumber} | Nazwa aukcji i cena: {buyNowPriceAndName}");
            }

            // Search button "next page"
            IWebElement nextPageButton = driver.FindElement(By.CssSelector("a[data-role='next-page']"));

            // Scroll window to "next page" button
            IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
            jse.ExecuteScript("arguments[0].scrollIntoView(true);", nextPageButton);

            // Click "next page" button
            IWebElement nextButton = driver.FindElement(By.CssSelector("a[data-role='next-page']"));
            nextButton = driver.FindElement(By.CssSelector("a[data-role='next-page']"));
            nextButton.Click();

            Thread.Sleep(milliseconds);
        }

        // Quit website
        driver.Quit();
    }

    // TODO scrolling like human
    static void ScrollToNextPageElement(IWebDriver driver, IWebElement nextButton)
    {
        Actions actions = new Actions(driver);
        actions.MoveToElement(nextButton).Perform();
        Thread.Sleep(1000);

        // scroll down the page so that the "next page" button is at the top of the view
        int windowHeight = driver.Manage().Window.Size.Height;
        int y = nextButton.Location.Y - windowHeight / 2;
        string script = $"window.scrollTo(0, {y});";
        ((IJavaScriptExecutor)driver).ExecuteScript(script);
        Thread.Sleep(1000);

        // scroll down the page gradually until the "next page" button disappears from view
        int scrollStep = windowHeight / 10;
        int remainingScrollDistance = nextButton.Location.Y - windowHeight;
        while (remainingScrollDistance > 0)
        {
            int scrollDistance = Math.Min(scrollStep, remainingScrollDistance);
            script = $"window.scrollBy(0, {scrollDistance});";
            ((IJavaScriptExecutor)driver).ExecuteScript(script);
            Thread.Sleep(250);
            remainingScrollDistance -= scrollDistance;
        }
    }

    static void ScrollToNextPage(IWebDriver driver)
    {
        int attempts = 0;
        while (attempts < 3)
        {
            try
            {
                var nextButton = driver.FindElement(By.CssSelector("a[data-role='next-page']"));
                if (nextButton.Displayed && nextButton.Enabled)
                {
                    Actions actions = new Actions(driver);
                    actions.MoveToElement(nextButton).Perform();
                    Thread.Sleep(1000);
                    actions.Click().Perform();
                    break;
                }
            }
            catch (StaleElementReferenceException)
            {
                // the button is no longer visible, please try again
            }
            attempts++;
        }
    }
}
