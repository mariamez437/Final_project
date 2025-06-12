document.addEventListener("DOMContentLoaded", function () {
  document.getElementById("fetchItems").addEventListener("click", async () => {
    const email = document.getElementById("email").value;
    const resultsDiv = document.getElementById("results");
    const itemsList = document.getElementById("items-list");
    const errorDiv = document.getElementById("error");

    // Clear previous results and errors
    itemsList.innerHTML = "";
    errorDiv.textContent = "";
    resultsDiv.style.display = "none";

    if (!email) {
      errorDiv.textContent = "Please enter your email";
      return;
    }

    try {
      // Show loading state
      document.getElementById("fetchItems").textContent = "Loading...";
      document.getElementById("fetchItems").disabled = true;

      const token = localStorage.getItem("token");
      const response = await fetch(
        `http://localhost:5194/api/Checking_For_Items/get-all-items?email=${encodeURIComponent(
          email
        )}`,
        {
          headers: {
            Authorization: `Bearer ${token}`, // Include JWT token in the header
          },
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const items = await response.json();

      // Display results
      if (items.length === 0) {
        itemsList.innerHTML = '<div class="item">No items found</div>';
      } else {
        items.forEach((item) => {
          itemElement.className = "item";
          itemElement.textContent = JSON.stringify(item, null, 2);
          itemsList.appendChild(itemElement);
        });
      }

      resultsDiv.style.display = "block";
    } catch (error) {
      console.error("Error fetching items:", error);
      errorDiv.textContent = `Failed to fetch items: ${error.message}`;
    } finally {
      // Reset button state
      document.getElementById("fetchItems").textContent = "Get All Items";
      document.getElementById("fetchItems").disabled = false;
    }
  });
});
