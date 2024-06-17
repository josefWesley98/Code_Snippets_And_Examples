#include "MapExample.h"
#include <iostream>
#include <map>
#include <cassert>

template<typename K, typename V>
class interval_map {
    friend void IntervalMapTest();
    V m_valBegin;
    std::map<K, V> m_map;
public:
    interval_map(V const& val)
        : m_valBegin(val)
    {}

    void assign(K const& keyBegin, K const& keyEnd, V const& val) {
        if (!(keyBegin < keyEnd)) return;

        V currentBeginVal;
        auto itBegin = m_map.find(keyBegin);
        if (itBegin != m_map.end()) {
            currentBeginVal = itBegin->second;
        }
        else {
            currentBeginVal = m_valBegin;
        }

        if (currentBeginVal != val) {
            m_map[keyBegin] = val;
        }

        auto itEnd = m_map.find(keyEnd);
        if (itEnd == m_map.end() || itEnd->first != keyEnd) {
            V currentEndVal;
            if (itEnd != m_map.end()) {
                currentEndVal = itEnd->second;
            }
            else {
                currentEndVal = m_valBegin;
            }
            m_map[keyEnd] = (currentEndVal != val) ? currentEndVal : m_valBegin;
        }

        itBegin = m_map.lower_bound(keyBegin);
        itEnd = m_map.upper_bound(keyEnd);
        if (std::next(itBegin) != itEnd) {
            auto first = std::next(itBegin);
            auto last = std::prev(itEnd);
            while (first != last) {
                m_map.erase(first++);
            }
        }

        if (itBegin != m_map.begin() && std::prev(itBegin)->second == itBegin->second) {
            m_map.erase(itBegin);
        }
        if (itEnd != m_map.end() && std::prev(itEnd)->second == itEnd->second) {
            m_map.erase(std::prev(itEnd));
        }
    }


    V const& operator[](K const& key) const {
        auto it = m_map.upper_bound(key);
        if (it == m_map.begin()) {
            return m_valBegin;
        }
        else {
            return (--it)->second;
        }
    }
};

void runTests() {
    interval_map<int, char> intervalMap1('A');
    intervalMap1.assign(0, 5, 'B');
    intervalMap1.assign(6, 10, 'C');
    intervalMap1.assign(3, 8, 'D');
    intervalMap1.assign(11, 14, 'D');

    for (int i = -1; i < 16; i++)
    {
        std::cout << "Value at " << i << ": " << intervalMap1[i] << std::endl;
    }
}

int main() {

    runTests();
    return 0;
}
