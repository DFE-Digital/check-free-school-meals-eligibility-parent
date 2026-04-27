let activeIndex = -1;
let currentResults = [];

function searchSchoolList(query) {
    const list = document.getElementById('schoolListResults');
    const input = document.getElementById('SelectedSchoolURN');

    if (query && query.length >= 3) {
        fetch('/Check/SearchSchools?query=' + encodeURIComponent(query))
            .then(response => {
                if (!response.ok) {
                    throw new Error('Search failed');
                }
                return response.json();
            })
            .then(data => {
                list.innerHTML = '';
                currentResults = data;
                activeIndex = -1;

                if (data.length === 0) {
                    closeList();
                    return;
                }

                data.forEach((value, index) => {
                    const li = document.createElement('li');

                    li.id = `school-option-${index}`;
                    li.setAttribute('role', 'option');
                    li.setAttribute('aria-selected', 'false');
                    li.className =
                        index % 2 === 0
                            ? 'autocomplete__option'
                            : 'autocomplete__option autocomplete__option--odd';

                    li.textContent = `${value.name}, ${value.id}, ${value.postcode}, ${value.la}`;

                    li.addEventListener('click', () => {
                        selectSchoolFromList(
                            value.name,
                            value.id,
                            value.la,
                            value.postcode,
                            value.inPrivateBeta
                        );
                    });

                    list.appendChild(li);
                });

                openList();
            })
            .catch(() => {
                list.innerHTML = '<li role="option">Error searching schools</li>';
                openList();
            });
    } else {
        closeList();
    }
}

function openList() {
    const list = document.getElementById('schoolListResults');
    const input = document.getElementById('SelectedSchoolURN');

    list.hidden = false;
    input.setAttribute('aria-expanded', 'true');
}

function closeList() {
    const list = document.getElementById('schoolListResults');
    const input = document.getElementById('SelectedSchoolURN');

    list.hidden = true;
    input.setAttribute('aria-expanded', 'false');
    input.removeAttribute('aria-activedescendant');
    activeIndex = -1;
}

function setActiveOption(index) {
    const list = document.getElementById('schoolListResults');
    const input = document.getElementById('SelectedSchoolURN');
    const options = list.querySelectorAll('[role="option"]');

    options.forEach(option => option.setAttribute('aria-selected', 'false'));

    const activeOption = options[index];
    if (activeOption) {
        activeOption.setAttribute('aria-selected', 'true');
        input.setAttribute('aria-activedescendant', activeOption.id);
        activeIndex = index;
    }
}

function handleKeyDown(event) {
    const list = document.getElementById('schoolListResults');
    const options = list.querySelectorAll('[role="option"]');

    if (list.hidden || options.length === 0) {
        return;
    }

    switch (event.key) {
        case 'ArrowDown':
            event.preventDefault();
            setActiveOption(
                activeIndex < options.length - 1 ? activeIndex + 1 : 0
            );
            break;

        case 'ArrowUp':
            event.preventDefault();
            setActiveOption(
                activeIndex > 0 ? activeIndex - 1 : options.length - 1
            );
            break;

        case 'Enter':
            event.preventDefault();
            if (activeIndex >= 0) {
                const value = currentResults[activeIndex];
                selectSchoolFromList(
                    value.name,
                    value.id,
                    value.la,
                    value.postcode,
                    value.inPrivateBeta
                );
            }
            break;

        case 'Escape':
            closeList();
            break;
    }
}

function selectSchoolFromList(school, urn, la, postcode, inPrivateBeta) {
    document.getElementById('SelectedSchoolURN').value =
        `${school}, ${urn}, ${postcode}, ${la}`;

    document.getElementById('SelectedSchoolURNHidden').value = urn;
    document.getElementById('SelectedSchoolName').value = school;
    document.getElementById('SelectedSchoolLA').value = la;
    document.getElementById('SelectedSchoolPostcode').value = postcode;
    document.getElementById('SelectedSchoolInPrivateBeta').value =
        inPrivateBeta === true ? 'true' : 'false';

    closeList();
}

/* Event listeners */
const schoolListSearchInput =
    document.querySelector('.school-list-search');

if (schoolListSearchInput) {
    schoolListSearchInput.addEventListener('input', function () {
        searchSchoolList(this.value);
    });

    schoolListSearchInput.addEventListener('keydown', handleKeyDown);
}