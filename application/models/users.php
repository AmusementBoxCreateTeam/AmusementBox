<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Users extends CI_Model {

    public function __construct() {
        parent::__construct();
        $this->load->database();
    }

    function get_list($offset='') {
        $this->db->select('users.id');
        $this->db->select('users.nickname');
        $this->db->select('users.birthday');
        $this->db->select('users.entry_date');
        $this->db->select('users.gender');
        $this->db->from('users');
        $this->db->join('historys', 'users.id = historys.user_id', 'left');
        $this->db->where('delete_date', null);
        $this->db->order_by('users.entry_date', 'desc');
        $this->db->order_by('users.id', 'desc');
        $this->db->limit(100,$offset);
        $query = $this->db->get();

        return $query->result();
    }

    function get_count_list() {
        $this->db->select('users.id');
        $this->db->select('users.nickname');
        $this->db->select('users.birthday');
        $this->db->select('users.entry_date');
        $this->db->select('users.gender');
        $this->db->from('users');
        $this->db->join('historys', 'users.id = historys.user_id', 'left');
        $this->db->where('delete_date', null);
        $this->db->order_by('users.entry_date', 'desc');
        $this->db->order_by('users.id', 'desc');

        $query = $this->db->get();

        return $query->num_rows();
    }

    function get_use_count($id) {
        $this->db->select('count(user_id) as use_count');
        $this->db->from('historys');
        $this->db->where('user_id', $id);
        $this->db->group_by('user_id');

        $query = $this->db->get();

        if ($query->num_rows() > 0) {
            return $query->row()->use_count;
        } else {
            return 0;
        }
    }

}
