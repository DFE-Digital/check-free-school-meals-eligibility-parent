document.addEventListener('DOMContentLoaded', () => {

    const activeIndex = {};
    const resultCache = {};
    const searchStateByChild = {};

    // Cancel any ongoing school search request
    function cancelSchoolSearch(index) {
        const search = searchStateByChild[index];
        clearTimeout(search.timer);
        search.controller?.abort();
        search.version++;
    }

    function openList(index) {
        const input = document.getElementById(`ChildList[${index}].School`);
        const list = document.getElementById(`schoolList${index}`);
        list.hidden = false;
        input.setAttribute('aria-expanded', 'true');
    }

    function closeList(index) {
        const input = document.getElementById(`ChildList[${index}].School`);
        const list = document.getElementById(`schoolList${index}`);
        list.hidden = true;
        list.innerHTML = '';
        input.setAttribute('aria-expanded', 'false');
        input.removeAttribute('aria-activedescendant');
        activeIndex[index] = -1;
    }

    function setActive(index, optionIndex) {
        const list = document.getElementById(`schoolList${index}`);
        const options = list.querySelectorAll('[role="option"]');

        options.forEach(o => o.setAttribute('aria-selected', 'false'));

        const option = options[optionIndex];
        if (!option) return;

        option.setAttribute('aria-selected', 'true');
        document
            .getElementById(`ChildList[${index}].School`)
            .setAttribute('aria-activedescendant', option.id);

        activeIndex[index] = optionIndex;
    }

    function searchSchool(query, index) {
        if (!query || query.length < 3) {
            closeList(index);
            return;
        }

        const search = searchStateByChild[index];
        const version = search.version;
        search.controller = new AbortController();
        fetch('/Check/SearchSchools?query=' + encodeURIComponent(query), { signal: search.controller.signal })
            .then(response => {
                if (!response.ok) throw new Error('Search failed');
                return response.json();
            })
            .then(data => {
                if (version !== search.version) return;

                const list = document.getElementById(`schoolList${index}`);
                list.innerHTML = '';
                resultCache[index] = data;
                activeIndex[index] = -1;

                if (data.length === 0) {
                    closeList(index);
                    return;
                }

                data.forEach((school, i) => {
                    const li = document.createElement('li');
                    li.id = `school-${index}-${i}`;
                    li.setAttribute('role', 'option');
                    li.setAttribute('aria-selected', 'false');
                    li.className = 'autocomplete__option';
                    li.textContent =
                        `${school.name}, ${school.id}, ${school.postcode}, ${school.la}`;

                    li.addEventListener('click', () =>
                        selectSchool(school, index));

                    list.appendChild(li);
                });

                openList(index);
            })
            .catch(() => {
                if (version !== search.version) return;

                const list = document.getElementById(`schoolList${index}`);
                list.innerHTML = '<li role="status">Error searching schools</li>';
                openList(index);
            });
    }

    function selectSchool(school, index) {
        cancelSchoolSearch(index);
        document.getElementById(`ChildList[${index}].School`).value =
            `${school.name}, ${school.id}, ${school.postcode}, ${school.la}`;

        // Submit the selected school's fields separately from the visible label.
        document.getElementById(`ChildList[${index}].School.Name`).value = school.name;
        document.getElementById(`ChildList[${index}].School.URN`).value = school.id;
        document.getElementById(`ChildList[${index}].School.Postcode`).value = school.postcode;
        document.getElementById(`ChildList[${index}].School.LA`).value = school.la;

        closeList(index);
    }

    function handleKeyDown(e, index) {
        const list = document.getElementById(`schoolList${index}`);
        if (e.key === 'Escape') {
            cancelSchoolSearch(index);
            closeList(index);
            return;
        }
        if (list.hidden) return;

        const options = list.querySelectorAll('[role="option"]');

        // Move through suggestions with arrow keys and select with Enter.
        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                setActive(index,
                    activeIndex[index] < options.length - 1
                        ? activeIndex[index] + 1 : 0);
                break;

            case 'ArrowUp':
                e.preventDefault();
                setActive(index,
                    activeIndex[index] > 0
                        ? activeIndex[index] - 1 : options.length - 1);
                break;

            case 'Enter':
                if (activeIndex[index] >= 0) {
                    e.preventDefault();
                    selectSchool(resultCache[index][activeIndex[index]], index);
                }
                break;
        }
    }

    document.querySelectorAll('.school-search').forEach((input, index) => {
        searchStateByChild[index] = { version: 0 };
        input.addEventListener('input', e => {
            cancelSchoolSearch(index);
            closeList(index);
            const query = e.target.value;
            // Search after typing pauses, once the query has at least three characters.
            if (query.length >= 3) {
                searchStateByChild[index].timer = setTimeout(() => searchSchool(query, index), 200);
            }
        });
        input.addEventListener('keydown', e => handleKeyDown(e, index));
    });

});