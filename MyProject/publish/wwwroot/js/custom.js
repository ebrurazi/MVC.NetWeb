function toggleSubmenu(menuId) {
    const submenu = document.getElementById(menuId);
    if (submenu.style.display === 'none' || submenu.style.display === '') {
        submenu.style.display = 'block';
    } else {
        submenu.style.display = 'none';
    }
}
window.onload = function() {
    if (document.getElementById('splash-screen')) {
        setTimeout(function() {
            document.getElementById('splash-screen').style.display = 'none';
            document.getElementById('main-content').style.display = 'block';
            document.getElementById('main-content-body').style.display = 'block';
            document.querySelector('.footer').style.display = 'block'; // Footer'ı göster
        }, 1500); // 1.5 saniye sonra splash screen kaybolacak
    } else {
        document.querySelector('.header').style.display = 'block';
        document.querySelector('.footer').style.display = 'block';
    }
}

function toggleSubmenu(menuId) {
    const submenu = document.getElementById(menuId);
    if (submenu.style.display === 'none' || submenu.style.display === '') {
        submenu.style.display = 'block';
    } else {
        submenu.style.display = 'none';
    }
}

function filterSolutions() {
    var keyword = document.getElementById('searchKeyword').value.toLowerCase();
    var date = document.getElementById('searchDate').value;
    var topic = document.getElementById('searchTopic').value.toLowerCase();

    var solutions = document.getElementsByClassName('solution-item');
    for (var i = 0; i < solutions.length; i++) {
        var solution = solutions[i];
        var title = solution.getElementsByTagName('h2')[0].textContent.toLowerCase();
        var problem = solution.getElementsByTagName('p')[3].textContent.toLowerCase();
        var solutionDate = solution.getElementsByTagName('p')[1].textContent.split(': ')[1];
        var solutionTopic = solution.getElementsByTagName('p')[2].textContent.split(': ')[1].toLowerCase();

        var keywordMatch = keyword === '' || title.includes(keyword) || problem.includes(keyword);
        var dateMatch = date === '' || solutionDate === date;
        var topicMatch = topic === '' || solutionTopic === topic;

        if (keywordMatch && dateMatch && topicMatch) {
            solution.style.display = '';
        } else {
            solution.style.display = 'none';
        }
    }
}
var currentStep = 0;
showStep(currentStep);

function showStep(n) {
    var steps = document.getElementsByClassName("step");
    steps[n].classList.add("active");
    document.getElementById("prevBtn").style.display = n == 0 ? "none" : "inline";
    document.getElementById("nextBtn").style.display = n == (steps.length - 1) ? "none" : "inline";
    document.getElementById("submitBtn").style.display = n == (steps.length - 1) ? "inline" : "none";
    updateStepIndicators(n);
    validateForm();
}

function nextPrev(n) {
    var steps = document.getElementsByClassName("step");
    if (n == 1 && !validateForm()) return false;
    steps[currentStep].classList.remove("active");
    currentStep += n;
    showStep(currentStep);
}

function updateStepIndicators(n) {
    var indicators = document.getElementsByClassName("step-indicator");
    for (var i = 0; i < indicators.length; i++) {
        indicators[i].classList.remove("active");
        indicators[i].classList.remove("completed");
        if (i < n) {
            indicators[i].classList.add("completed");
        }
        if (i == n) {
            indicators[i].classList.add("active");
        }
    }
}

function validateForm() {
    var steps = document.getElementsByClassName("step");
    var inputs = steps[currentStep].getElementsByTagName("input");
    var textareas = steps[currentStep].getElementsByTagName("textarea");
    var selects = steps[currentStep].getElementsByTagName("select");
    var valid = true;

    for (var i = 0; i < inputs.length; i++) {
        if (inputs[i].hasAttribute("required") && inputs[i].value == "") {
            inputs[i].classList.add("is-invalid");
            inputs[i].classList.remove("is-valid");
            valid = false;
        } else {
            inputs[i].classList.add("is-valid");
            inputs[i].classList.remove("is-invalid");
        }
    }

    for (var i = 0; i < textareas.length; i++) {
        if (textareas[i].hasAttribute("required") && textareas[i].value == "") {
            textareas[i].classList.add("is-invalid");
            textareas[i].classList.remove("is-valid");
            valid = false;
        } else {
            textareas[i].classList.add("is-valid");
            textareas[i].classList.remove("is-invalid");
        }
    }

    for (var i = 0; i < selects.length; i++) {
        if (selects[i].hasAttribute("required") && selects[i].value == "") {
            selects[i].classList.add("is-invalid");
            selects[i].classList.remove("is-valid");
            valid = false;
        } else {
            selects[i].classList.add("is-valid");
            selects[i].classList.remove("is-invalid");
        }
    }

    document.getElementById("nextBtn").disabled = !valid;
    document.getElementById("submitBtn").disabled = !valid;
    return valid;
}

// Add event listeners to validate inputs in real-time
document.addEventListener("DOMContentLoaded", function() {
    var inputs = document.querySelectorAll("input[required], textarea[required], select[required]");
    inputs.forEach(function(input) {
        input.addEventListener("input", validateForm);
    });
});
document.getElementById('photos').addEventListener('change', function (event) {
    var files = event.target.files;
    var previewContainer = document.querySelector('.preview-container');
    previewContainer.innerHTML = ''; // Clear previous previews

    if (files.length > 3) {
        alert('En fazla 3 resim yükleyebilirsiniz.');
        document.getElementById('photos').value = ''; // Clear the file input
        return;
    }

    for (var i = 0; i < files.length; i++) {
        var file = files[i];
        var reader = new FileReader();
        reader.onload = function (e) {
            var imgContainer = document.createElement('div');
            imgContainer.classList.add('img-container');

            var img = document.createElement('img');
            img.src = e.target.result;
            img.classList.add('preview-image');

            var removeBtn = document.createElement('button');
            removeBtn.innerText = 'X';
            removeBtn.classList.add('remove-btn');
            removeBtn.addEventListener('click', function () {
                imgContainer.remove();
            });

            imgContainer.appendChild(img);
            imgContainer.appendChild(removeBtn);
            previewContainer.appendChild(imgContainer);
        };
        reader.readAsDataURL(file);
    }
});
function applyFilter() {
    var searchInput = document.getElementById('searchInput').value.toLowerCase();
    var filterSelect = document.getElementById('filterSelect').value;

    var solutionItems = document.querySelectorAll('.solution-item');

    solutionItems.forEach(function(item) {
        var title = item.querySelector('h2').innerText.toLowerCase();
        var category = item.getAttribute('data-category');

        var matchesSearch = title.includes(searchInput);
        var matchesFilter = filterSelect === 'all' || filterSelect === category;

        if (matchesSearch && matchesFilter) {
            item.style.display = 'block';
        } else {
            item.style.display = 'none';
        }
    });
}
var currentStep = 0;
showStep(currentStep);

function showStep(n) {
    var steps = document.getElementsByClassName("step");
    steps[n].style.display = "block";
    if (n == 0) {
        document.getElementById("prevBtn").disabled = true;
    } else {
        document.getElementById("prevBtn").disabled = false;
    }
    if (n == (steps.length - 1)) {
        document.getElementById("nextBtn").style.display = "none";
        document.getElementById("submitBtn").style.display = "inline";
    } else {
        document.getElementById("nextBtn").style.display = "inline";
        document.getElementById("submitBtn").style.display = "none";
    }
}

function nextPrev(n) {
    var steps = document.getElementsByClassName("step");
    steps[currentStep].style.display = "none";
    currentStep = currentStep + n;
    if (currentStep >= steps.length) {
        document.getElementById("solutionForm").submit();
        return false;
    }
    showStep(currentStep);
}
function searchSolutions() {
    var searchInput = document.getElementById("searchInput").value.toLowerCase();
    var categoryFilter = document.getElementById("categoryFilter").value;

    var solutions = document.querySelectorAll(".solution-item");

    solutions.forEach(function(solution) {
        var title = solution.querySelector("h2").innerText.toLowerCase();
        var category = solution.getAttribute("data-category");

        if (title.includes(searchInput) && (categoryFilter === "Genel" || category === categoryFilter)) {
            solution.style.display = "";
        } else {
            solution.style.display = "none";
        }
    });
}
