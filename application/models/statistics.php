<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Statistics extends CI_Model {

    public function __construct() {
        parent::__construct();
        $this->load->database();
        $this->load->library('conversion_age_birthday');
    }

    public function get_rank5($search) {
        $this->db->select('songs.song_title');
        $this->db->select('songs.lyricist');
        $this->db->select('songs.composer');
        $this->db->select('songs.singer');
        $this->db->select('songs.genre');
        $this->db->from('historys');
        $this->db->join('users', 'historys.user_id = users.id', 'left');
        $this->db->join('songs', 'historys.song_id = songs.id', 'left');
        $where = $this->rank5_where($search);
        $this->db->where($where);

        $query = $this->db->get();
        return $query->result();
    }

    private function rank5_where($search) {
        $where = array();

        if (!empty($search['gender'])) {
            $where += array('users.gender' => $search['gender']);
        }
        if (!empty($search['age'])) {
            $age_over = $this->conversion_age_birthday->conversion_date($search['age']);
            $age_under = $this->conversion_age_birthday->conversion_date($search['age']+9);
            $where += array('users.birthday <=' => $age_over);
            $where += array('users.birthday >=' => $age_under);
        }
        return $where;
    }

}
