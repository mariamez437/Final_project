document.addEventListener("DOMContentLoaded", function () {
  const signUpForm = document.querySelector(".sign-up-form");
  if (signUpForm) {
    signUpForm.addEventListener("submit", async function (event) {
      console.log("Hello signUpForm");
      event.preventDefault();

      // القيم
      const UserName = document.getElementById("username").value.trim();
      const Email = document.getElementById("email").value.trim();
      const Password = document.getElementById("password").value;
      const PhoneNumber = document.getElementById("phone").value.trim();
      const CardID = document.getElementById("card").value.trim();

      // عناصر عرض الأخطاء
      const nameError = document.getElementById("nameError");
      const emailError = document.getElementById("emailError");
      const phoneError = document.getElementById("phoneError");
      const cardError = document.getElementById("cardError");
      const passwordError = document.getElementById("passwordError");

      // تصفير الأخطاء
      nameError.textContent = "";
      emailError.textContent = "";
      phoneError.textContent = "";
      cardError.textContent = "";
      passwordError.textContent = "";

      let isValid = true;
      const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

      // التحقق من الاسم
      if (UserName === "") {
        nameError.textContent = "Username is required.";
        nameError.classList.add("text-danger");
        isValid = false;
      }

      // التحقق من الإيميل
      if (Email === "") {
        emailError.textContent = "Email is required.";
        emailError.classList.add("text-danger");
        isValid = false;
      } else if (!emailPattern.test(Email)) {
        emailError.textContent = "Invalid email format.";
        emailError.classList.add("text-danger");
        isValid = false;
      }

      // التحقق من رقم الهاتف
      const phonePattern = /^(010|011|012|015)[0-9]{8}$/;

      if (PhoneNumber === "") {
        phoneError.textContent = "Phone number is required.";
        phoneError.classList.add("text-danger");
        isValid = false;
      } else if (!phonePattern.test(PhoneNumber)) {
        phoneError.textContent = "Enter a valid Egyptian phone number.";
        phoneError.classList.add("text-danger");
        isValid = false;
      }

      // التحقق من الرقم القومي / كارت
      const cardPattern = /^\d{14}$/;
      // التحقق من الرقم القومي
      if (CardID === "") {
        cardError.textContent = "Card ID is required.";
        cardError.classList.add("text-danger");
        isValid = false;
      } else if (!cardPattern.test(CardID)) {
        cardError.textContent = "Card ID must be 14 digits.";
        cardError.classList.add("text-danger");
        isValid = false;
      }

      // التحقق من كلمة السر
      if (Password === "") {
        passwordError.textContent = "Password is required.";
        passwordError.classList.add("text-danger");
        isValid = false;
      } else if (Password.length < 8) {
        passwordError.textContent = "Password must be at least 8 characters.";
        passwordError.classList.add("text-danger");
        isValid = false;
      }

      if (isValid) {
        try {
          // Show loading state
          const submitBtn = document.getElementById("submit");

          const formData = new FormData();
          formData.append("UserName", UserName);
          formData.append("Email", Email);
          formData.append("Password", Password);
          formData.append("PhoneNumber", PhoneNumber);
          formData.append("CardID", CardID);

          // Make API request
          const response = await fetch(
            "http://localhost:5194/api/Auth/Register",
            {
              method: "POST",
              body: formData,
            }
          );

          if (response.ok) {
            // Redirect to login page
            window.location.replace("login.html");
          } else {
            const errorData = await response.json();
            // alert(errorData.message || "Registration failed");
            ShowBootstrapToast(errorData.message, "danger");
          }
        } catch (error) {
          console.error("Registration error:", error);
          ShowBootstrapToast(
            `Registration error in Phone Number Or Email Or ID must be unique: ${error}`,
            "danger"
          );
        } finally {
          // Reset button state
          const submitBtn = document.getElementById("submit");
          if (submitBtn) {
            submitBtn.value = originalBtnText;
            submitBtn.disabled = false;
          }
        }
      }
    });
  }

  function updateAuthUI() {
    console.log("updateAuthUI");
    const authItem = document.getElementById("auth-item");
    const authLink = document.getElementById("auth-link");
    const token = localStorage.getItem("token");

    if (token) {
      // User is logged in - show Logout
      authLink.textContent = "Logout";
      authLink.href = "#";
      authLink.onclick = function (e) {
        e.preventDefault();
        localStorage.removeItem("token");
        window.location.replace("../index.html"); // Redirect to home after logout
      };
    } else {
      // User is not logged in - show Login
      authLink.textContent = "Login";
      authLink.href = "/htmlStaticFiles/Registeration Pages/login.html";
      authLink.onclick = null;
    }
  }

  updateAuthUI();

  // Function to handle logout
  function handleLogout(e) {
    // e.preventDefault(); // Prevent default link behavior
    localStorage.removeItem("token"); // Remove the token
    window.location.href = "../index.html"; // Redirect to home page
  }

  // Get the stored user type from localStorage
  const userType = localStorage.getItem("type");
  const managerLink = document.getElementById("manager-link");
  console.log(userType);
  console.log(managerLink);
  // Hide Manager link if the user is not a manager
  if (userType !== "Manager" && managerLink) {
    managerLink.style.display = "none";
  }

  // حفظ بيانات العناصر المفقودة
  const lostItems = JSON.parse(localStorage.getItem("lostItems")) || [];
  const itemList = document.getElementById("itemList");
  const addItemForm = document.querySelector(".container form");

  function saveItems() {
    localStorage.setItem("lostItems", JSON.stringify(lostItems));
  }

  function renderItems(items) {
    if (!itemList) return;
    itemList.innerHTML = "";
    items.forEach((item, index) => {
      const li = document.createElement("li");
      li.textContent = `${item.type}: ${item.details}, Location: ${item.location}`;
      const deleteBtn = document.createElement("button");
      deleteBtn.textContent = "Delete";
      deleteBtn.onclick = function () {
        lostItems.splice(index, 1);
        saveItems();
        renderItems(lostItems);
      };
      li.appendChild(deleteBtn);
      itemList.appendChild(li);
    });
  }

  if (addItemForm) {
    addItemForm.addEventListener("submit", function (event) {
      event.preventDefault();
      const type = addItemForm.querySelector("h2 span").textContent;
      const details = addItemForm.querySelector("input[type='text']").value;
      const location = addItemForm.querySelector(
        "input[placeholder='p-number']"
      ).value;
      if (type && details && location) {
        lostItems.push({ type, details, location });
        saveItems();
        alert("Item registered successfully!");
        addItemForm.reset();
      }
    });
  }

  renderItems(lostItems);

  // Contact Us Form
  const contactForm = document.querySelector("#contact form");
  if (contactForm) {
    contactForm.addEventListener("submit", function (event) {
      event.preventDefault();
      const name = document.getElementById("name").value;
      const email = document.getElementById("email").value;
      const subject = document.getElementById("subject").value;
      const message = document.getElementById("mesage").value;

      if (name && email && message) {
        const contactMessages =
          JSON.parse(localStorage.getItem("contactMessages")) || [];
        contactMessages.push({ name, email, subject, message });
        localStorage.setItem(
          "contactMessages",
          JSON.stringify(contactMessages)
        );
        alert("Your message has been sent successfully!");
        contactForm.reset();
      } else {
        alert("Please fill in all required fields.");
      }
    });
  }
});
window.ShowBootstrapToast = function (
  message,
  type = "Info",
  withButtons = false
) {
  const toastId = "custom-toast-" + Date.now();
  const toastHTML = `
        <div id="${toastId}" class="toast align-items-center text-white bg-${type.toLowerCase()} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body w-100">
                    ${message}
                    ${
                      withButtons
                        ? `
                    <div class="mt-2 pt-2 border-top d-flex justify-content-end gap-2">
                        <button type="button" class="btn btn-light btn-sm" id="btn-add-new">Add Another</button>
                        <button type="button" class="btn btn-outline-light btn-sm" id="btn-go-home">Go Home</button>
                    </div>`
                        : ""
                    }
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

  let toastContainer = document.getElementById("toast-container");
  if (!toastContainer) {
    toastContainer = document.createElement("div");
    toastContainer.id = "toast-container";
    toastContainer.className =
      "toast-container position-fixed bottom-0 end-0 p-3";
    document.body.appendChild(toastContainer);
  }

  toastContainer.innerHTML += toastHTML;

  const toastElement = document.getElementById(toastId);
  const toast = new bootstrap.Toast(toastElement, { delay: 7000 });
  toast.show();

  if (withButtons) {
    toastElement
      .querySelector("#btn-add-new")
      .addEventListener("click", function () {
        $("#register-card-2-form")[0].reset();
        $("#preview").attr(
          "src",
          "images/id-card-illustration_23-2147829294.avif"
        );
        $(".error-msg").text("");
        toast.hide();
      });

    toastElement
      .querySelector("#btn-go-home")
      .addEventListener("click", function () {
        window.location.replace("../../index.html");
      });
  }

  toastElement.addEventListener("hidden.bs.toast", function () {
    toastElement.remove();
  });
};
